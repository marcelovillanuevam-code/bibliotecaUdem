import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthSessionService } from '../../../../core/services/auth-session.service';
import { MockLibraryDataService } from '../../../../core/services/mock-library-data.service';
import { AvatarChipComponent } from '../../../../shared/ui/avatar-chip/avatar-chip.component';

@Component({
  selector: 'app-dashboard-page',
  imports: [RouterLink, AvatarChipComponent],
  templateUrl: './dashboard-page.component.html',
  styleUrl: './dashboard-page.component.scss'
})
export class DashboardPageComponent {
  protected readonly authSession = inject(AuthSessionService);
  protected readonly libraryData = inject(MockLibraryDataService);
  protected readonly currentUser = this.authSession.currentUser;

  protected statIcon(icon: string): string {
    switch (icon) {
      case 'books':
        return 'M6.5 5.5H10.5C11.3284 5.5 12 6.17157 12 7V19C12 18.1716 11.3284 17.5 10.5 17.5H6.5C5.67157 17.5 5 18.1716 5 19V7C5 6.17157 5.67157 5.5 6.5 5.5ZM13.5 5.5H17.5C18.3284 5.5 19 6.17157 19 7V19C19 18.1716 18.3284 17.5 17.5 17.5H13.5C12.6716 17.5 12 18.1716 12 19V7C12 6.17157 12.6716 5.5 13.5 5.5Z';
      case 'users':
        return 'M16 20V18.8C16 17.806 15.194 17 14.2 17H8.8C7.80589 17 7 17.806 7 18.8V20M17.5 7.5C17.5 9.15685 16.1569 10.5 14.5 10.5C12.8431 10.5 11.5 9.15685 11.5 7.5C11.5 5.84315 12.8431 4.5 14.5 4.5C16.1569 4.5 17.5 5.84315 17.5 7.5ZM12.5 8C12.5 9.38071 11.3807 10.5 10 10.5C8.61929 10.5 7.5 9.38071 7.5 8C7.5 6.61929 8.61929 5.5 10 5.5C11.3807 5.5 12.5 6.61929 12.5 8Z';
      case 'available':
        return 'M6 12.5L10 16.5L18 7.5M12 21C16.9706 21 21 16.9706 21 12C21 7.02944 16.9706 3 12 3C7.02944 3 3 7.02944 3 12C3 16.9706 7.02944 21 12 21Z';
      case 'search':
        return 'M21 21L16.65 16.65M18 10.5C18 14.6421 14.6421 18 10.5 18C6.35786 18 3 14.6421 3 10.5C3 6.35786 6.35786 3 10.5 3C14.6421 3 18 6.35786 18 10.5Z';
      default:
        return 'M8 7H16M8 11H16M8 15H12M7 3H17C18.1046 3 19 3.89543 19 5V19L16 17L13 19L10 17L7 19V5C7 3.89543 7.89543 3 9 3Z';
    }
  }

  protected activityIcon(icon: string): string {
    switch (icon) {
      case 'book':
        return this.statIcon('books');
      case 'user':
        return this.statIcon('users');
      case 'return':
        return 'M8 10L12 14L16 10M12 4V14M6 18H18';
      default:
        return 'M12 8V12M12 16H12.01M10.29 3.86L1.82 18A2 2 0 0 0 3.55 21H20.45A2 2 0 0 0 22.18 18L13.71 3.86A2 2 0 0 0 10.29 3.86Z';
    }
  }
}
