import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuthSessionService } from '../../services/auth-session.service';
import { FinesApiService } from '../../services/fines-api.service';
import { MockLibraryDataService } from '../../services/mock-library-data.service';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { TopbarComponent } from '../topbar/topbar.component';
import { SupportFabComponent } from '../../../shared/ui/support-fab/support-fab.component';

const LIBRARIAN_ROUTES = new Set(['/dashboard/libros', '/usuarios', '/dashboard/devoluciones']);
const ADMIN_ROUTES = new Set(['/dashboard/configuracion/multas', '/dashboard/reportes']);

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, SidebarComponent, TopbarComponent, SupportFabComponent],
  templateUrl: './app-shell.component.html',
  styleUrl: './app-shell.component.scss'
})
export class AppShellComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);
  private readonly authSession = inject(AuthSessionService);
  private readonly finesApi = inject(FinesApiService);
  protected readonly libraryData = inject(MockLibraryDataService);
  protected readonly currentUser = this.authSession.currentUser;
  protected readonly sidebarOpen = signal(false);
  protected readonly pendingFinesCount = signal(0);

  protected readonly navItems = computed(() => {
    const roleCode = this.authSession.currentUserRoleCode();
    const isAdmin = roleCode === 'ADMIN';
    const canManage = isAdmin || roleCode === 'LIBRARIAN';

    return this.libraryData.navItems.filter(item => {
      if (ADMIN_ROUTES.has(item.route)) return isAdmin;
      if (LIBRARIAN_ROUTES.has(item.route)) return canManage;
      return true;
    });
  });

  constructor() {
    if (typeof window !== 'undefined') {
      const syncState = (): void => {
        this.sidebarOpen.set(window.innerWidth >= 1024);
      };

      syncState();
      const listener = (): void => syncState();
      window.addEventListener('resize', listener);
      this.destroyRef.onDestroy(() => window.removeEventListener('resize', listener));
    }

    this.loadPendingFinesCount();
  }

  protected get pageTitle(): string {
    const url = this.router.url;

    if (url.startsWith('/usuarios')) return 'Usuarios';
    if (url.startsWith('/dashboard/configuracion/multas')) return 'Configuracion de Multas';
    if (url.startsWith('/dashboard/devoluciones/nueva')) return 'Nueva Devolucion';
    if (url.startsWith('/dashboard/devoluciones')) return 'Devoluciones';
    if (url.startsWith('/dashboard/multas/')) return 'Detalle Multa';
    if (url === '/dashboard/multas') return 'Multas';
    if (url.startsWith('/dashboard/catalogo')) return 'Buscar Libros';
    if (url.startsWith('/dashboard/libros') && url.includes('/reservas')) return 'Cola de Reservas';
    if (url.startsWith('/dashboard/libros')) return 'Gestion Libros';
    if (url.startsWith('/dashboard/reservas')) return 'Mis Reservas';
    if (url.startsWith('/dashboard/prestamos')) return 'Prestamos';
    if (url.startsWith('/dashboard/reportes')) return 'Reportes';
    if (url.startsWith('/dashboard/perfil')) return 'Mi Perfil';

    return 'Dashboard';
  }

  private loadPendingFinesCount(): void {
    const userId = this.authSession.currentUserId();
    if (!userId) return;

    this.finesApi
      .getPendingCount(userId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (count) => this.pendingFinesCount.set(count),
        error: () => {}
      });
  }

  protected toggleSidebar(): void {
    this.sidebarOpen.update((value) => !value);
  }

  protected closeSidebar(): void {
    if (typeof window !== 'undefined' && window.innerWidth < 1024) {
      this.sidebarOpen.set(false);
    }
  }

  protected logout(): void {
    this.authSession.logout();
    this.router.navigateByUrl('/login');
  }
}
