import { Component, DestroyRef, inject, signal } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { AuthSessionService } from '../../services/auth-session.service';
import { MockLibraryDataService } from '../../services/mock-library-data.service';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { TopbarComponent } from '../topbar/topbar.component';
import { SupportFabComponent } from '../../../shared/ui/support-fab/support-fab.component';

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
  protected readonly libraryData = inject(MockLibraryDataService);
  protected readonly currentUser = this.authSession.currentUser;
  protected readonly sidebarOpen = signal(false);

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
  }

  protected get pageTitle(): string {
    if (this.router.url.startsWith('/usuarios')) {
      return 'Usuarios';
    }

    if (this.router.url.startsWith('/dashboard/catalogo')) {
      return 'Buscar Libros';
    }

    if (this.router.url.startsWith('/dashboard/libros')) {
      return 'Gestión Libros';
    }

    if (this.router.url.startsWith('/dashboard/perfil')) {
      return 'Mi Perfil';
    }

    return 'Dashboard';
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
