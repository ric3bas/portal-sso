import { Pencil } from 'lucide-react'
import { useEffect, useRef, useState, type FormEvent } from 'react'
import { Badge, Button, CheckboxField, Feedback, Modal, PageIntro, Panel } from '../components/ui'
import { getErrorMessage } from '../lib/errors'
import { parceirosApi, perfisApi, UsuariosApi } from '../services/sso'
import type { ParceiroResponse, PerfilComEscopoResponse, SelectOption, UsuarioComPerfilResponse } from '../types/api'

interface UserFormState {
  nome: string
  email: string
  login: string
  senha: string
  perfil: string
  parceiro: string
  ativo: boolean
  bloqueado: boolean
}

interface UserFormErrors {
  nome: string
  email: string
  login: string
  senha: string
  perfil: string
  parceiro: string
}

const initialFormState: UserFormState = {
  nome: '',
  email: '',
  login: '',
  senha: '',
  perfil: '',
  parceiro: '',
  ativo: true,
  bloqueado: false,
}

const initialFormErrors: UserFormErrors = {
  nome: '',
  email: '',
  login: '',
  senha: '',
  perfil: '',
  parceiro: '',
}

function resolvePartnerValue(user: UsuarioComPerfilResponse, partners: ParceiroResponse[]) {
  if (user.parceiroId && partners.some((item) => item.id === user.parceiroId)) {
    return user.parceiroId
  }

  if (user.parceiro && partners.some((item) => item.id === user.parceiro)) {
    return user.parceiro
  }

  const matchedPartner = partners.find((item) => item.nome === user.parceiro)
  return matchedPartner?.id ?? ''
}

function resolveProfileValue(user: UsuarioComPerfilResponse, profiles: PerfilComEscopoResponse[]) {
  if (typeof user.perfilId === 'number' && profiles.some((item) => item.id === user.perfilId)) {
    return String(user.perfilId)
  }

  if (typeof user.perfil === 'number' && profiles.some((item) => item.id === user.perfil)) {
    return String(user.perfil)
  }

  if (typeof user.perfil === 'string') {
    const numericValue = Number(user.perfil)

    if (!Number.isNaN(numericValue) && profiles.some((item) => item.id === numericValue)) {
      return String(numericValue)
    }

    const matchedProfile = profiles.find((item) => item.nome === user.perfil)
    return matchedProfile ? String(matchedProfile.id) : ''
  }

  return ''
}

function validatePassword(password: string) {
  if (!password.trim()) {
    return 'O campo senha é obrigatório.'
  }

  if (password.length < 6) {
    return 'Campo Senha deve conter no minimo 6 caracteres'
  }

  if (password.length > 100) {
    return 'Campo Senha deve conter no maximo 100 caracteres'
  }

  if (!/[A-Z]/.test(password)) {
    return 'Campo Senha deve conter ao menos uma letra maiuscula'
  }

  if (!/[0-9]/.test(password)) {
    return 'Campo Senha deve conter ao menos um numero'
  }

  return ''
}

function validateForm(form: UserFormState, isEditing: boolean) {
  const nextErrors: UserFormErrors = { ...initialFormErrors }

  if (!form.nome.trim()) {
    nextErrors.nome = 'O campo nome é obrigatório.'
  }

  if (!form.email.trim()) {
    nextErrors.email = 'O campo e-mail é obrigatório.'
  }

  if (!form.login.trim()) {
    nextErrors.login = 'O campo login é obrigatório.'
  }

  if (!isEditing) {
    nextErrors.senha = validatePassword(form.senha)
  }

  if (!form.parceiro) {
    nextErrors.parceiro = 'O campo parceiro é obrigatório.'
  }

  if (!form.perfil) {
    nextErrors.perfil = 'O campo perfil é obrigatório.'
  }

  return nextErrors
}

function hasErrors(errors: UserFormErrors) {
  return Object.values(errors).some(Boolean)
}

function fieldClass(error: string) {
  return [
    'w-full rounded-md border bg-white px-3 py-2 text-sm text-slate-900 outline-none transition-colors placeholder:text-slate-400',
    error
      ? 'border-red-500 focus:border-red-600 focus:ring-2 focus:ring-red-100 dark:border-red-400 dark:focus:border-red-300 dark:focus:ring-red-950'
      : 'border-slate-300 focus:border-slate-900 focus:ring-2 focus:ring-slate-200 dark:border-slate-300 dark:focus:border-slate-900 dark:focus:ring-slate-200',
  ].join(' ')
}

export function UsuariosPage() {
  const didLoadInitiallyRef = useRef(false)
  const [items, setItems] = useState<UsuarioComPerfilResponse[]>([])
  const [parceiros, setParceiros] = useState<ParceiroResponse[]>([])
  const [perfis, setPerfis] = useState<PerfilComEscopoResponse[]>([])
  const [selectedPartnerFilter, setSelectedPartnerFilter] = useState('')
  const [feedback, setFeedback] = useState<{ tone: 'success' | 'danger'; message: string } | null>(null)
  const [tableMessage, setTableMessage] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [isSaving, setIsSaving] = useState(false)
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [editingItem, setEditingItem] = useState<UsuarioComPerfilResponse | null>(null)
  const [form, setForm] = useState<UserFormState>(initialFormState)
  const [formErrors, setFormErrors] = useState<UserFormErrors>(initialFormErrors)

  const partnerOptions: SelectOption[] = [{ label: 'Todos os parceiros', value: '' }].concat(
    parceiros.map((item) => ({ label: item.nome ?? item.id, value: item.id })),
  )

  const partnerFormOptions: SelectOption[] = [{ label: 'Selecione um parceiro', value: '' }].concat(
    parceiros.map((item) => ({ label: item.nome ?? item.id, value: item.id })),
  )

  const perfilOptions: SelectOption[] = [{ label: 'Selecione um perfil', value: '' }].concat(
    perfis.map((item) => ({ label: item.nome ?? `Perfil ${item.id}`, value: String(item.id) })),
  )

  async function loadReferenceData() {
    const [loadedPartners, loadedProfiles] = await Promise.all([parceirosApi.list(), perfisApi.list()])
    setParceiros(loadedPartners)
    setPerfis(loadedProfiles)
  }

  async function loadUsers(parceiroId?: string) {
    setIsLoading(true)

    try {
      const response = await UsuariosApi.list(parceiroId)
      setItems(response)
      setTableMessage(response.length === 0 ? 'Nenhum usuario encontrado' : null)
    } catch (error) {
      setItems([])
      setTableMessage(getErrorMessage(error, 'Falha ao carregar usuarios.'))
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    if (didLoadInitiallyRef.current) {
      return
    }

    didLoadInitiallyRef.current = true

    async function bootstrap() {
      setIsLoading(true)

      try {
        await Promise.all([loadReferenceData(), loadUsers()])
      } catch (error) {
        setFeedback({ tone: 'danger', message: getErrorMessage(error, 'Falha ao carregar dados iniciais de usuarios.') })
        setIsLoading(false)
      }
    }

    void bootstrap()
  }, [])

  function clearFieldError(field: keyof UserFormErrors) {
    setFormErrors((current) => ({ ...current, [field]: '' }))
  }

  function openCreateModal() {
    setEditingItem(null)
    setForm(initialFormState)
    setFormErrors(initialFormErrors)
    setIsModalOpen(true)
  }

  function openEditModal(item: UsuarioComPerfilResponse) {
    setEditingItem(item)
    setForm({
      nome: item.nome ?? '',
      email: item.email ?? '',
      login: item.login ?? '',
      senha: '',
      perfil: resolveProfileValue(item, perfis),
      parceiro: resolvePartnerValue(item, parceiros),
      ativo: item.ativo ?? true,
      bloqueado: item.bloqueado ?? false,
    })
    setFormErrors(initialFormErrors)
    setIsModalOpen(true)
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setFeedback(null)

    const validationErrors = validateForm(form, Boolean(editingItem))
    setFormErrors(validationErrors)

    if (hasErrors(validationErrors)) {
      return
    }

    setIsSaving(true)

    try {
      if (editingItem) {
        await UsuariosApi.update(editingItem.id, {
          nome: form.nome.trim(),
          email: form.email.trim(),
          login: form.login.trim(),
          parceiro: form.parceiro,
          perfil: Number(form.perfil),
          ativo: form.ativo,
          bloqueado: form.bloqueado,
        })
        setFeedback({ tone: 'success', message: 'Usuario atualizado com sucesso.' })
      } else {
        await UsuariosApi.create({
          nome: form.nome.trim(),
          email: form.email.trim(),
          login: form.login.trim(),
          senha: form.senha,
          parceiro: form.parceiro,
          perfil: Number(form.perfil),
        })
        setFeedback({ tone: 'success', message: 'Usuario registrado com sucesso.' })
      }

      setForm(initialFormState)
      setFormErrors(initialFormErrors)
      setEditingItem(null)
      setIsModalOpen(false)
      await loadUsers(selectedPartnerFilter || undefined)
    } catch (error) {
      setFeedback({ tone: 'danger', message: getErrorMessage(error, editingItem ? 'Falha ao atualizar usuario.' : 'Falha ao registrar usuario.') })
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <div className="space-y-6">
      <PageIntro
        action={<Button onClick={openCreateModal}>Novo</Button>}
        eyebrow=""
        title="Usuários"
        description=""
      />

      {feedback ? <Feedback tone={feedback.tone}>{feedback.message}</Feedback> : null}

      <Panel className="p-5 md:p-6">
        <div className="max-w-xs">
          <select
            className="w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900 outline-none transition-colors focus:border-slate-900 focus:ring-2 focus:ring-slate-200 dark:border-slate-300 dark:bg-white dark:text-slate-900 dark:focus:border-slate-900 dark:focus:ring-slate-200"
            onChange={async (event) => {
              const nextValue = event.target.value
              setSelectedPartnerFilter(nextValue)
              await loadUsers(nextValue || undefined)
            }}
            value={selectedPartnerFilter}
          >
            {partnerOptions.map((option) => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </select>
        </div>

        {isLoading ? (
          <div className="mt-6 rounded-md border border-[var(--line)] bg-[var(--surface-strong)] px-5 py-8 text-sm text-[var(--text-soft)]">Carregando usuarios...</div>
        ) : (
          <div className="mt-6 overflow-hidden rounded-md border border-[var(--line)] dark:border-slate-300">
            <div className="overflow-x-auto">
              <table className="min-w-full border-collapse text-left text-sm">
                <thead className="bg-[var(--surface-strong)] text-[var(--text-soft)] dark:bg-white dark:text-slate-700">
                  <tr>
                    <th className="px-4 py-3 font-semibold">Id</th>
                    <th className="px-4 py-3 font-semibold">Nome</th>
                    <th className="px-4 py-3 font-semibold">Login</th>
                    <th className="px-4 py-3 font-semibold">E-mail</th>
                    <th className="px-4 py-3 font-semibold">Parceiro</th>
                    <th className="px-4 py-3 font-semibold">Perfil</th>
                    <th className="px-4 py-3 font-semibold">Ativo</th>
                    <th className="px-4 py-3 font-semibold">Bloqueado</th>
                    <th className="px-4 py-3 font-semibold text-right">Acoes</th>
                  </tr>
                </thead>
                <tbody>
                  {items.length > 0 ? (
                    items.map((item) => (
                      <tr key={item.id} className="border-t border-[var(--line)] bg-white/70 dark:border-slate-300 dark:bg-white">
                        <td className="px-4 py-3 font-mono-ui text-xs text-[var(--text-soft)] dark:text-slate-500">#{item.id}</td>
                        <td className="px-4 py-3 font-medium text-[var(--text)] dark:text-slate-900">{item.nome}</td>
                        <td className="px-4 py-3 text-[var(--text-soft)] dark:text-slate-600">{item.login}</td>
                        <td className="px-4 py-3 text-[var(--text-soft)] dark:text-slate-600">{item.email}</td>
                        <td className="px-4 py-3 text-[var(--text-soft)] dark:text-slate-600">{item.parceiro ?? item.parceiroId ?? '-'}</td>
                        <td className="px-4 py-3 text-[var(--text-soft)] dark:text-slate-600">{String(item.perfil ?? item.perfilId ?? '-')}</td>
                        <td className="px-4 py-3">
                          <Badge tone={item.ativo ? 'success' : 'danger'}>{item.ativo ? 'Sim' : 'Nao'}</Badge>
                        </td>
                        <td className="px-4 py-3">
                          <Badge tone={item.bloqueado ? 'danger' : 'success'}>{item.bloqueado ? 'Sim' : 'Nao'}</Badge>
                        </td>
                        <td className="px-4 py-3 text-right">
                          <Button
                            aria-label="Editar usuario"
                            className="h-11 w-11 px-0"
                            onClick={() => openEditModal(item)}
                            title="Editar"
                            type="button"
                            variant="secondary"
                          >
                            <Pencil className="h-5 w-5" />
                          </Button>
                        </td>
                      </tr>
                    ))
                  ) : (
                    <tr className="border-t border-[var(--line)] bg-white/70 dark:border-slate-300 dark:bg-white">
                      <td className="px-4 py-6 text-sm text-[var(--text-soft)] dark:text-slate-600" colSpan={9}>
                        {tableMessage ?? 'Nenhum usuario encontrado'}
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
        onClose={() => setIsModalOpen(false)}
        open={isModalOpen}
        title={editingItem ? 'Editar usuario' : 'Novo usuario'}
      >
        <form className="space-y-5" noValidate onSubmit={handleSubmit}>
          <div className="grid gap-4 md:grid-cols-2">
            <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
              <span>
                Nome <span className="text-red-600 dark:text-red-400">*</span>
              </span>
              <input
                className={fieldClass(formErrors.nome)}
                onChange={(event) => setForm((current) => ({ ...current, nome: event.target.value }))}
                onFocus={() => clearFieldError('nome')}
                placeholder="Informe o nome"
                value={form.nome}
              />
              {formErrors.nome ? <span className="text-xs text-red-600 dark:text-red-400">{formErrors.nome}</span> : null}
            </label>

            <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
              <span>
                E-mail <span className="text-red-600 dark:text-red-400">*</span>
              </span>
              <input
                className={fieldClass(formErrors.email)}
                onChange={(event) => setForm((current) => ({ ...current, email: event.target.value }))}
                onFocus={() => clearFieldError('email')}
                placeholder="Informe o e-mail"
                type="email"
                value={form.email}
              />
              {formErrors.email ? <span className="text-xs text-red-600 dark:text-red-400">{formErrors.email}</span> : null}
            </label>

            <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
              <span>
                Login <span className="text-red-600 dark:text-red-400">*</span>
              </span>
              <input
                className={fieldClass(formErrors.login)}
                onChange={(event) => setForm((current) => ({ ...current, login: event.target.value }))}
                onFocus={() => clearFieldError('login')}
                placeholder="Informe o login"
                value={form.login}
              />
              {formErrors.login ? <span className="text-xs text-red-600 dark:text-red-400">{formErrors.login}</span> : null}
            </label>

            {!editingItem ? (
              <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
                <span>
                  Senha <span className="text-red-600 dark:text-red-400">*</span>
                </span>
                <input
                  className={fieldClass(formErrors.senha)}
                  onChange={(event) => setForm((current) => ({ ...current, senha: event.target.value }))}
                  onFocus={() => clearFieldError('senha')}
                  placeholder="Informe a senha"
                  type="password"
                  value={form.senha}
                />
                {formErrors.senha ? <span className="text-xs text-red-600 dark:text-red-400">{formErrors.senha}</span> : null}
              </label>
            ) : null}

            <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
              <span>
                Parceiro <span className="text-red-600 dark:text-red-400">*</span>
              </span>
              <select
                className={fieldClass(formErrors.parceiro)}
                onChange={(event) => {
                  setForm((current) => ({ ...current, parceiro: event.target.value }))
                  clearFieldError('parceiro')
                }}
                onFocus={() => clearFieldError('parceiro')}
                value={form.parceiro}
              >
                {partnerFormOptions.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
              {formErrors.parceiro ? <span className="text-xs text-red-600 dark:text-red-400">{formErrors.parceiro}</span> : null}
            </label>

            <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
              <span>
                Perfil <span className="text-red-600 dark:text-red-400">*</span>
              </span>
              <select
                className={fieldClass(formErrors.perfil)}
                onChange={(event) => {
                  setForm((current) => ({ ...current, perfil: event.target.value }))
                  clearFieldError('perfil')
                }}
                onFocus={() => clearFieldError('perfil')}
                value={form.perfil}
              >
                {perfilOptions.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
              {formErrors.perfil ? <span className="text-xs text-red-600 dark:text-red-400">{formErrors.perfil}</span> : null}
            </label>

            {editingItem ? (
              <div className="grid gap-4 md:col-span-2 md:grid-cols-2">
                <CheckboxField
                  checked={form.ativo}
                  label="Usuario ativo"
                  onChange={(event) => setForm((current) => ({ ...current, ativo: event.target.checked }))}
                />
                <CheckboxField
                  checked={form.bloqueado}
                  label="Usuario bloqueado"
                  onChange={(event) => setForm((current) => ({ ...current, bloqueado: event.target.checked }))}
                />
              </div>
            ) : null}
          </div>

          <div className="flex justify-end gap-3 border-t border-slate-200 pt-4 dark:border-slate-800">
            <Button onClick={() => setIsModalOpen(false)} type="button" variant="ghost">
              Cancelar
            </Button>
            <Button disabled={isSaving} type="submit">
              {isSaving ? 'Salvando...' : editingItem ? 'Salvar' : 'Salvar'}
            </Button>
          </div>
        </form>
      </Modal>
    </div>
  )
}