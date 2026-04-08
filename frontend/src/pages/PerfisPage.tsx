import { Copy, Pencil, Trash2 } from 'lucide-react'
import { useEffect, useMemo, useRef, useState, type FormEvent } from 'react'
import { Button, Feedback, Modal, PageIntro, Panel } from '../components/ui'
import { getErrorMessage } from '../lib/errors'
import { escoposApi, perfisApi } from '../services/sso'
import type { EscopoResponse, PerfilComEscopoResponse } from '../types/api'

interface ScopeTreeNode {
  key: string
  label: string
  children: ScopeTreeNode[]
  scopeId?: number
}

type ProfileScopeSelections = Record<number, number[]>
type ProfileExpandedNodes = Record<number, string[]>

function normalizeScopeName(name?: string | null) {
  const value = name?.trim()
  return value && value.length > 0 ? value : 'escopo-sem-nome'
}

function sortTree(node: ScopeTreeNode) {
  node.children.sort((left, right) => left.label.localeCompare(right.label, 'pt-BR'))
  node.children.forEach(sortTree)
  return node
}

function buildScopeTree(escopos: EscopoResponse[]) {
  const root: ScopeTreeNode = {
    key: 'root',
    label: 'Todos os escopos',
    children: [],
  }

  escopos.forEach((escopo) => {
    const scopeName = normalizeScopeName(escopo.nome)
    const parts = scopeName.split('.').filter(Boolean)

    if (parts.length === 0) {
      root.children.push({
        key: `scope-${escopo.id}`,
        label: scopeName,
        children: [],
        scopeId: escopo.id,
      })
      return
    }

    let current = root

    parts.forEach((part, index) => {
      const key = parts.slice(0, index + 1).join('.')
      let child = current.children.find((item) => item.key === key)

      if (!child) {
        child = {
          key,
          label: part,
          children: [],
        }
        current.children.push(child)
      }

      if (index === parts.length - 1) {
        child.scopeId = escopo.id
      }

      current = child
    })
  })

  return sortTree(root)
}

function collectScopeIds(node: ScopeTreeNode): number[] {
  const ownScopeId = node.scopeId !== undefined ? [node.scopeId] : []

  return ownScopeId.concat(node.children.flatMap(collectScopeIds))
}

function toggleNodeScopes(selectedIds: number[], node: ScopeTreeNode, checked: boolean) {
  const next = new Set(selectedIds)

  collectScopeIds(node).forEach((scopeId) => {
    if (checked) {
      next.add(scopeId)
      return
    }

    next.delete(scopeId)
  })

  return Array.from(next).sort((left, right) => left - right)
}

function areSelectionsEqual(left: number[], right: number[]) {
  if (left.length !== right.length) {
    return false
  }

  const orderedLeft = [...left].sort((a, b) => a - b)
  const orderedRight = [...right].sort((a, b) => a - b)

  return orderedLeft.every((value, index) => value === orderedRight[index])
}

function buildProfileScopeSelections(items: PerfilComEscopoResponse[]) {
  return Object.fromEntries(items.map((item) => [item.id, (item.escopos ?? []).map((escopo) => escopo.id).sort((a, b) => a - b)])) as ProfileScopeSelections
}

function buildDefaultExpandedNodes(profileId: number, node: ScopeTreeNode) {
  const expandableKeys = node.children.filter((child) => child.children.length > 0).map((child) => child.key)

  return {
    [profileId]: ['root', ...expandableKeys],
  } satisfies ProfileExpandedNodes
}

function ScopeTreeBranch({
  node,
  depth,
  selectedIds,
  onToggle,
  expandedKeys,
  onToggleExpand,
}: {
  node: ScopeTreeNode
  depth: number
  selectedIds: number[]
  onToggle: (node: ScopeTreeNode, checked: boolean) => void
  expandedKeys: string[]
  onToggleExpand: (nodeKey: string) => void
}) {
  const inputRef = useRef<HTMLInputElement>(null)
  const scopeIds = collectScopeIds(node)
  const selectedCount = scopeIds.filter((scopeId) => selectedIds.includes(scopeId)).length
  const checked = scopeIds.length > 0 && selectedCount === scopeIds.length
  const indeterminate = selectedCount > 0 && selectedCount < scopeIds.length
  const isRoot = depth === 0
  const isExpandable = node.children.length > 0
  const isExpanded = expandedKeys.includes(node.key)

  useEffect(() => {
    if (inputRef.current) {
      inputRef.current.indeterminate = indeterminate
    }
  }, [indeterminate])

  return (
    <div className="space-y-0.5">
      <label
        className={
          isRoot
            ? 'flex items-center justify-between gap-2 border border-[var(--line)] bg-[var(--surface-strong)] px-2.5 py-1.5 dark:border-slate-300 dark:bg-white'
            : 'flex items-center gap-2 px-1 py-0.5'
        }
      >
        <span className="flex items-center gap-2">
          {isExpandable ? (
            <button
              aria-label={isExpanded ? `Fechar ${node.label}` : `Abrir ${node.label}`}
              className="inline-flex h-4 w-4 items-center justify-center border border-[var(--line)] bg-white text-xs font-semibold leading-none text-[var(--text-soft)] dark:border-slate-300 dark:text-slate-600"
              onClick={(event) => {
                event.preventDefault()
                onToggleExpand(node.key)
              }}
              type="button"
            >
              {isExpanded ? '-' : '+'}
            </button>
          ) : (
            <span className="inline-block h-4 w-4" />
          )}
          <input
            checked={checked}
            className="h-4 w-4 accent-[var(--brand)]"
            onChange={(event) => onToggle(node, event.target.checked)}
            ref={inputRef}
            type="checkbox"
          />
          <span className={isRoot ? 'text-sm font-semibold text-[var(--text)] dark:text-slate-900' : 'text-sm leading-5 text-[var(--text)] dark:text-slate-900'}>{node.label}</span>
        </span>
        {isRoot ? <span className="text-xs font-medium text-[var(--text-soft)] dark:text-slate-500">{selectedCount}/{scopeIds.length} selecionados</span> : null}
      </label>

      {node.children.length > 0 && isExpanded ? (
        <div className={isRoot ? 'ml-2 space-y-0.5 border-l border-[var(--line)] pl-2.5 dark:border-slate-300' : 'ml-2 space-y-0.5 border-l border-[var(--line)] pl-2.5 dark:border-slate-300'}>
          {node.children.map((child) => (
            <ScopeTreeBranch
              depth={depth + 1}
              expandedKeys={expandedKeys}
              key={child.key}
              node={child}
              onToggle={onToggle}
              onToggleExpand={onToggleExpand}
              selectedIds={selectedIds}
            />
          ))}
        </div>
      ) : null}
    </div>
  )
}

export function PerfisPage() {
  const didLoadInitiallyRef = useRef(false)
  const [items, setItems] = useState<PerfilComEscopoResponse[]>([])
  const [escopos, setEscopos] = useState<EscopoResponse[]>([])
  const [feedback, setFeedback] = useState<{ tone: 'success' | 'danger'; message: string } | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [isSaving, setIsSaving] = useState(false)
  const [actionProfileId, setActionProfileId] = useState<number | null>(null)
  const [savingProfileId, setSavingProfileId] = useState<number | null>(null)
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false)
  const [isEditModalOpen, setIsEditModalOpen] = useState(false)
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false)
  const [profilePendingDelete, setProfilePendingDelete] = useState<PerfilComEscopoResponse | null>(null)
  const [selectedProfile, setSelectedProfile] = useState<PerfilComEscopoResponse | null>(null)
  const [editingNameProfileId, setEditingNameProfileId] = useState<number | null>(null)
  const [editingNameValue, setEditingNameValue] = useState('')
  const [nome, setNome] = useState('')
  const [nomeError, setNomeError] = useState('')
  const [profileScopeSelections, setProfileScopeSelections] = useState<ProfileScopeSelections>({})
  const [savedProfileScopeSelections, setSavedProfileScopeSelections] = useState<ProfileScopeSelections>({})
  const [expandedNodes, setExpandedNodes] = useState<ProfileExpandedNodes>({})
  const scopeTree = useMemo(() => buildScopeTree(escopos), [escopos])

  async function loadData() {
    setIsLoading(true)

    try {
      const [perfis, listaEscopos] = await Promise.all([perfisApi.list(), escoposApi.list()])
      const selections = buildProfileScopeSelections(perfis)

      setItems(perfis)
      setEscopos(listaEscopos)
      setProfileScopeSelections(selections)
      setSavedProfileScopeSelections(selections)
      setExpandedNodes((current) => {
        const next: ProfileExpandedNodes = {}

        perfis.forEach((perfil) => {
          next[perfil.id] = current[perfil.id] ?? ['root']
        })

        return next
      })
    } catch (error) {
      setFeedback({ tone: 'danger', message: getErrorMessage(error, 'Falha ao carregar perfis.') })
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    if (didLoadInitiallyRef.current) {
      return
    }

    didLoadInitiallyRef.current = true
    void loadData()
  }, [])

  function updateProfileScopes(profileId: number, node: ScopeTreeNode, checked: boolean) {
    setProfileScopeSelections((current) => ({
      ...current,
      [profileId]: toggleNodeScopes(current[profileId] ?? [], node, checked),
    }))
  }

  async function openEditModal(profile: PerfilComEscopoResponse) {
    setIsLoading(true)

    try {
      const fullProfile = await perfisApi.getById(profile.id)
      const selectedIds = (fullProfile.escopos ?? []).map((escopo) => escopo.id).sort((left, right) => left - right)

      setSelectedProfile(fullProfile)
      setProfileScopeSelections((current) => ({
        ...current,
        [fullProfile.id]: selectedIds,
      }))
      setSavedProfileScopeSelections((current) => ({
        ...current,
        [fullProfile.id]: selectedIds,
      }))
      setExpandedNodes((current) => ({
        ...current,
        ...buildDefaultExpandedNodes(fullProfile.id, scopeTree),
      }))
      setIsEditModalOpen(true)
    } catch (error) {
      setFeedback({ tone: 'danger', message: getErrorMessage(error, 'Falha ao carregar o perfil selecionado.') })
    } finally {
      setIsLoading(false)
    }
  }

  function toggleExpandedNode(profileId: number, nodeKey: string) {
    setExpandedNodes((current) => {
      const profileExpandedNodes = current[profileId] ?? ['root']
      const nextProfileExpandedNodes = profileExpandedNodes.includes(nodeKey)
        ? profileExpandedNodes.filter((item) => item !== nodeKey)
        : [...profileExpandedNodes, nodeKey]

      return {
        ...current,
        [profileId]: nextProfileExpandedNodes,
      }
    })
  }

  async function handleCreateProfile(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setFeedback(null)

    if (!nome.trim()) {
      setNomeError('O campo nome é obrigatório.')
      return
    }

    setIsSaving(true)

    try {
      await perfisApi.create({ nome: nome.trim() })
      setFeedback({ tone: 'success', message: 'Perfil criado com sucesso.' })
      setNome('')
      setNomeError('')
      setIsCreateModalOpen(false)
      await loadData()
    } catch (error) {
      setFeedback({ tone: 'danger', message: getErrorMessage(error, 'Falha ao criar perfil.') })
    } finally {
      setIsSaving(false)
    }
  }

  async function handleSaveProfileScopes(profile: PerfilComEscopoResponse) {
    setFeedback(null)
    setSavingProfileId(profile.id)

    const selectedScopeIds = profileScopeSelections[profile.id] ?? []

    try {
      await perfisApi.vincularEscopos(profile.id, { escopoIds: selectedScopeIds })

      const updatedEscopos = escopos
        .filter((escopo) => selectedScopeIds.includes(escopo.id))
        .map((escopo) => ({ id: escopo.id, nome: escopo.nome }))

      setItems((current) => current.map((item) => (item.id === profile.id ? { ...item, escopos: updatedEscopos } : item)))
      setSavedProfileScopeSelections((current) => ({
        ...current,
        [profile.id]: [...selectedScopeIds].sort((a, b) => a - b),
      }))
      setSelectedProfile((current) => (current?.id === profile.id ? { ...current, escopos: updatedEscopos } : current))
      setIsEditModalOpen(false)
      setFeedback({ tone: 'success', message: 'Escopos vinculados com sucesso.' })
    } catch (error) {
      setFeedback({ tone: 'danger', message: getErrorMessage(error, 'Falha ao vincular escopos.') })
    } finally {
      setSavingProfileId(null)
    }
  }

  async function handleCloneProfile(profileId: number) {
    setFeedback(null)
    setActionProfileId(profileId)

    try {
      await perfisApi.clone(profileId)
      await loadData()
      setFeedback({ tone: 'success', message: 'Perfil clonado com sucesso.' })
    } catch (error) {
      setFeedback({ tone: 'danger', message: getErrorMessage(error, 'Falha ao clonar perfil.') })
    } finally {
      setActionProfileId(null)
    }
  }

  function openNameEditor(profile: PerfilComEscopoResponse) {
    setEditingNameProfileId(profile.id)
    setEditingNameValue(profile.nome ?? '')
  }

  function closeNameEditor() {
    setEditingNameProfileId(null)
    setEditingNameValue('')
  }

  async function handleRenameProfile(profile: PerfilComEscopoResponse) {
    const trimmedName = editingNameValue.trim()

    if (!trimmedName) {
      setFeedback({ tone: 'danger', message: 'O nome do perfil e obrigatorio.' })
      return
    }

    if (trimmedName === (profile.nome ?? '').trim()) {
      closeNameEditor()
      return
    }

    setFeedback(null)
    setActionProfileId(profile.id)

    try {
      await perfisApi.updateName(profile.id, trimmedName)

      setItems((current) => current.map((item) => (item.id === profile.id ? { ...item, nome: trimmedName } : item)))
      setSelectedProfile((current) => (current?.id === profile.id ? { ...current, nome: trimmedName } : current))
      closeNameEditor()
      setFeedback({ tone: 'success', message: 'Nome do perfil atualizado com sucesso.' })
    } catch (error) {
      setFeedback({ tone: 'danger', message: getErrorMessage(error, 'Falha ao atualizar nome do perfil.') })
    } finally {
      setActionProfileId(null)
    }
  }

  function openDeleteModal(profile: PerfilComEscopoResponse) {
    setProfilePendingDelete(profile)
    setIsDeleteModalOpen(true)
  }

  async function handleDeleteProfile() {
    if (!profilePendingDelete) {
      return
    }

    setFeedback(null)
    setActionProfileId(profilePendingDelete.id)

    try {
      await perfisApi.delete(profilePendingDelete.id)

      if (selectedProfile?.id === profilePendingDelete.id) {
        setIsEditModalOpen(false)
        setSelectedProfile(null)
      }

      await loadData()
      setIsDeleteModalOpen(false)
      setProfilePendingDelete(null)
      setFeedback({ tone: 'success', message: 'Perfil apagado com sucesso.' })
    } catch (error) {
      setFeedback({ tone: 'danger', message: getErrorMessage(error, 'Falha ao apagar perfil.') })
    } finally {
      setActionProfileId(null)
    }
  }

  return (
    <div className="space-y-6">
      <PageIntro
        action={<Button onClick={() => {
          setNome('')
          setNomeError('')
          setIsCreateModalOpen(true)
        }}>Novo</Button>}
        eyebrow=""
        title="Perfis"
        description=""
      />

      {feedback ? <Feedback tone={feedback.tone}>{feedback.message}</Feedback> : null}

      <Panel className="p-4">
        {isLoading ? (
          <div className="border border-[var(--line)] px-4 py-6 text-sm text-[var(--text-soft)]">Carregando perfis...</div>
        ) : (
            <div className="overflow-hidden rounded-md border border-[var(--line)] bg-white dark:border-slate-300 dark:bg-white">
            <div className="overflow-x-auto">
              <table className="min-w-full border-collapse text-left text-sm">
                <thead className="bg-[var(--surface-strong)] text-[var(--text-soft)] dark:bg-white dark:text-slate-700">
                  <tr>
                    <th className="px-4 py-3 font-semibold">Id</th>
                    <th className="px-4 py-3 font-semibold">Nome</th>
                    <th className="px-4 py-3 font-semibold">Escopos</th>
                    <th className="px-4 py-3 font-semibold text-right">Acoes</th>
                  </tr>
                </thead>
                <tbody>
                  {items.length > 0 ? (
                    items.map((item) => (
                      <tr key={item.id} className="border-t border-[var(--line)] bg-white align-top dark:border-slate-300 dark:bg-white">
                        <td className="px-4 py-3 text-xs text-[var(--text-soft)] dark:text-slate-500">#{item.id}</td>
                        <td className="px-4 py-3 font-medium text-[var(--text)] dark:text-slate-900">
                          {editingNameProfileId === item.id ? (
                            <input
                              autoFocus
                              className="w-full rounded-md border border-slate-300 bg-white px-2.5 py-1.5 text-sm text-slate-900 outline-none transition-colors focus:border-slate-900 focus:ring-2 focus:ring-slate-200"
                              onBlur={closeNameEditor}
                              onChange={(event) => setEditingNameValue(event.target.value)}
                              onKeyDown={(event) => {
                                if (event.key === 'Enter') {
                                  event.preventDefault()
                                  void handleRenameProfile(item)
                                }

                                if (event.key === 'Escape') {
                                  event.preventDefault()
                                  closeNameEditor()
                                }
                              }}
                              value={editingNameValue}
                            />
                          ) : (
                            <button
                              className="cursor-pointer text-left font-medium text-[var(--text)] transition-colors hover:text-[var(--brand)] dark:text-slate-900 dark:hover:text-sky-600"
                              disabled={actionProfileId === item.id}
                              onClick={() => openNameEditor(item)}
                              title="Clique para editar o nome"
                              type="button"
                            >
                              {item.nome}
                            </button>
                          )}
                        </td>
                        <td className="px-4 py-3 text-[var(--text-soft)] dark:text-slate-600">
                          {(item.escopos?.length ?? 0) > 0
                            ? `${item.escopos?.length ?? 0} escopo(s)`
                            : 'Nenhum escopo vinculado'}
                        </td>
                        <td className="px-4 py-3 text-right">
                          <div className="flex justify-end gap-2">
                            <Button
                              aria-label="Editar perfil"
                              className="h-11 w-11 px-0"
                              disabled={actionProfileId === item.id}
                              onClick={() => void openEditModal(item)}
                              title="Editar"
                              type="button"
                              variant="secondary"
                            >
                              <Pencil className="h-5 w-5" />
                            </Button>
                            <Button
                              aria-label="Clonar perfil"
                              className="h-11 w-11 px-0"
                              disabled={actionProfileId === item.id}
                              onClick={() => void handleCloneProfile(item.id)}
                              title="Clonar"
                              type="button"
                              variant="ghost"
                            >
                              <Copy className="h-5 w-5" />
                            </Button>
                            <Button
                              aria-label="Apagar perfil"
                              className="h-11 w-11 px-0"
                              disabled={actionProfileId === item.id}
                              onClick={() => openDeleteModal(item)}
                              title="Apagar"
                              type="button"
                              variant="danger"
                            >
                              <Trash2 className="h-5 w-5" />
                            </Button>
                          </div>
                        </td>
                      </tr>
                    ))
                  ) : (
                    <tr className="border-t border-[var(--line)] bg-white dark:border-slate-300 dark:bg-white">
                      <td className="px-4 py-6 text-sm text-[var(--text-soft)] dark:text-slate-600" colSpan={4}>
                        Nenhum perfil encontrado
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </Panel>

      <Modal
        onClose={() => {
          setIsEditModalOpen(false)
          setSelectedProfile(null)
        }}
        open={isEditModalOpen}
        title="Editar perfil"
      >
        {selectedProfile ? (
          <div className="space-y-4">
            <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
              <span>Perfil</span>
              <input
                className="w-full rounded-md border border-slate-300 bg-slate-50 px-3 py-2 text-sm text-slate-700 outline-none dark:border-slate-300 dark:bg-slate-100 dark:text-slate-900"
                disabled
                value={selectedProfile.nome ?? ''}
              />
            </label>

            <div className="space-y-2">
              <p className="text-sm font-medium text-slate-700 dark:text-slate-200">Escopos</p>
              {escopos.length > 0 ? (
                <div className="max-h-[48vh] overflow-y-auto rounded-md border border-[var(--line)] bg-white p-2.5 dark:border-slate-300 dark:bg-white">
                  <ScopeTreeBranch
                    depth={0}
                    expandedKeys={expandedNodes[selectedProfile.id] ?? ['root']}
                    node={scopeTree}
                    onToggle={(node, checked) => updateProfileScopes(selectedProfile.id, node, checked)}
                    onToggleExpand={(nodeKey) => toggleExpandedNode(selectedProfile.id, nodeKey)}
                    selectedIds={profileScopeSelections[selectedProfile.id] ?? []}
                  />
                </div>
              ) : (
                <span className="text-sm text-[var(--text-soft)] dark:text-slate-600">Nenhum escopo cadastrado.</span>
              )}
            </div>

            <div className="sticky bottom-0 -mx-6 flex justify-end gap-3 border-t border-slate-200 bg-white px-6 pt-3 pb-1 dark:border-slate-800 dark:bg-slate-950 md:-mx-8 md:px-8">
              <Button
                onClick={() => {
                  setIsEditModalOpen(false)
                  setSelectedProfile(null)
                }}
                type="button"
                variant="ghost"
              >
                Cancelar
              </Button>
              <Button
                disabled={
                  savingProfileId === selectedProfile.id ||
                  areSelectionsEqual(profileScopeSelections[selectedProfile.id] ?? [], savedProfileScopeSelections[selectedProfile.id] ?? [])
                }
                onClick={() => void handleSaveProfileScopes(selectedProfile)}
                type="button"
              >
                {savingProfileId === selectedProfile.id ? 'Salvando...' : 'Salvar'}
              </Button>
            </div>
          </div>
        ) : null}
      </Modal>

      <Modal
        onClose={() => setIsCreateModalOpen(false)}
        open={isCreateModalOpen}
        title="Novo perfil"
      >
        <form className="space-y-5" noValidate onSubmit={handleCreateProfile}>
          <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
            <span>
              Nome <span className="text-red-600 dark:text-red-400">*</span>
            </span>
            <input
              className={[
                'w-full rounded-md border bg-white px-3 py-2 text-sm text-slate-900 outline-none transition-colors placeholder:text-slate-400 dark:bg-white dark:text-slate-900 dark:placeholder:text-slate-500',
                nomeError
                  ? 'border-red-500 focus:border-red-600 focus:ring-2 focus:ring-red-100 dark:border-red-400 dark:focus:border-red-300 dark:focus:ring-red-950'
                  : 'border-slate-300 focus:border-slate-900 focus:ring-2 focus:ring-slate-200 dark:border-slate-700 dark:focus:border-slate-100 dark:focus:ring-slate-800',
              ].join(' ')}
              onChange={(event) => setNome(event.target.value)}
              onFocus={() => setNomeError('')}
              placeholder="Informe o nome"
              value={nome}
            />
            {nomeError ? <span className="text-xs text-red-600 dark:text-red-400">{nomeError}</span> : null}
          </label>
          <div className="flex justify-end gap-3 border-t border-slate-200 pt-4 dark:border-slate-800">
            <Button onClick={() => setIsCreateModalOpen(false)} type="button" variant="ghost">
              Cancelar
            </Button>
            <Button disabled={isSaving} type="submit">
              {isSaving ? 'Salvando...' : 'Criar perfil'}
            </Button>
          </div>
        </form>
      </Modal>

      <Modal
        onClose={() => {
          setIsDeleteModalOpen(false)
          setProfilePendingDelete(null)
        }}
        open={isDeleteModalOpen}
        title="Apagar perfil"
      >
        <div className="space-y-5">
          <p className="text-lg font-medium leading-7 text-slate-700 dark:text-slate-200">
            Deseja realmente apagar o perfil{' '}
            <strong className="font-semibold text-slate-900 dark:text-slate-100">
              {profilePendingDelete?.nome ?? (profilePendingDelete ? `#${profilePendingDelete.id}` : '')}
            </strong>
            ?
          </p>
          <div className="flex justify-end gap-3 border-t border-slate-200 pt-4 dark:border-slate-800">
            <Button
              onClick={() => {
                setIsDeleteModalOpen(false)
                setProfilePendingDelete(null)
              }}
              type="button"
              variant="ghost"
            >
              Cancelar
            </Button>
            <Button
              disabled={profilePendingDelete ? actionProfileId === profilePendingDelete.id : false}
              onClick={() => void handleDeleteProfile()}
              type="button"
              variant="danger"
            >
              {profilePendingDelete && actionProfileId === profilePendingDelete.id ? 'Apagando...' : 'Apagar'}
            </Button>
          </div>
        </div>
      </Modal>
    </div>
  )
}