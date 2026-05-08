import { Component, input, output } from '@angular/core';
import { NotificationBellComponent } from '../../../features/notificaciones/components/notification-bell/notification-bell.component';
import { CurrentUser } from '../../../shared/models/dashboard.model';

@Component({
  selector: 'app-topbar',
  imports: [NotificationBellComponent],
  templateUrl: './topbar.component.html',
  styleUrl: './topbar.component.scss'
})
export class TopbarComponent {
  readonly title = input.required<string>();
  readonly user = input.required<CurrentUser>();
  readonly pendingFinesCount = input(0);
  readonly menuToggle = output<void>();
  readonly logoutRequest = output<void>();
}
