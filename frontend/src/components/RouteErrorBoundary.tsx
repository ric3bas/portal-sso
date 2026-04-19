import { Component, type ErrorInfo, type PropsWithChildren, type ReactNode } from 'react'
import { useLocation } from 'react-router-dom'
import { Button, Panel } from './ui'

interface BoundaryProps extends PropsWithChildren {
  resetKey: string
}

interface BoundaryState {
  hasError: boolean
}

class RouteErrorBoundaryInner extends Component<BoundaryProps, BoundaryState> {
  override state: BoundaryState = {
    hasError: false,
  }

  static getDerivedStateFromError() {
    return { hasError: true }
  }

  override componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.error('Route rendering failed', error, errorInfo)
  }

  override componentDidUpdate(prevProps: BoundaryProps) {
    if (prevProps.resetKey !== this.props.resetKey && this.state.hasError) {
      this.setState({ hasError: false })
    }
  }

  private handleRetry = () => {
    this.setState({ hasError: false })
  }

  override render(): ReactNode {
    if (!this.state.hasError) {
      return this.props.children
    }

    return (
      <Panel className="p-6 md:p-8">
        <div className="space-y-4">
          <div>
            <h2 className="text-2xl font-semibold text-slate-900 dark:text-slate-100">Nao foi possivel renderizar esta pagina</h2>
            <p className="mt-2 text-sm text-slate-600 dark:text-slate-300">
              Ocorreu um erro de runtime nesta rota. Tente abrir outra opcao do menu ou recarregar esta pagina.
            </p>
          </div>
          <Button onClick={this.handleRetry} type="button" variant="secondary">
            Tentar novamente
          </Button>
        </div>
      </Panel>
    )
  }
}

export function RouteErrorBoundary({ children }: PropsWithChildren) {
  const location = useLocation()

  return <RouteErrorBoundaryInner resetKey={location.pathname}>{children}</RouteErrorBoundaryInner>
}