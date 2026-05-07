import { Component, input, output } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { CurrentUser, NavIcon, NavItem } from '../../../shared/models/dashboard.model';

@Component({
  selector: 'app-sidebar',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss'
})
export class SidebarComponent {
  readonly items = input.required<readonly NavItem[]>();
  readonly currentUser = input.required<CurrentUser>();
  readonly open = input(true);
  readonly closeRequest = output<void>();
  readonly logoutRequest = output<void>();

  protected trackByLabel = (_: number, item: NavItem): string => item.label;

  protected iconPath(icon: NavIcon): string {
    switch (icon) {
      case 'dashboard':
        return 'M4 5.5C4 4.67157 4.67157 4 5.5 4H10.5C11.3284 4 12 4.67157 12 5.5V10.5C12 11.3284 11.3284 12 10.5 12H5.5C4.67157 12 4 11.3284 4 10.5V5.5ZM4 15.5C4 14.6716 4.67157 14 5.5 14H10.5C11.3284 14 12 14.6716 12 15.5V20.5C12 21.3284 11.3284 22 10.5 22H5.5C4.67157 22 4 21.3284 4 20.5V15.5ZM14 5.5C14 4.67157 14.6716 4 15.5 4H20.5C21.3284 4 22 4.67157 22 5.5V10.5C22 11.3284 21.3284 12 20.5 12H15.5C14.6716 12 14 11.3284 14 10.5V5.5ZM14 15.5C14 14.6716 14.6716 14 15.5 14H20.5C21.3284 14 22 14.6716 22 15.5V20.5C22 21.3284 21.3284 22 20.5 22H15.5C14.6716 22 14 21.3284 14 20.5V15.5Z';
      case 'users':
        return 'M16 21V19C16 17.8954 15.1046 17 14 17H8C6.89543 17 6 17.8954 6 19V21M18 7C18 8.65685 16.6569 10 15 10C13.3431 10 12 8.65685 12 7C12 5.34315 13.3431 4 15 4C16.6569 4 18 5.34315 18 7ZM12 7C12 8.65685 10.6569 10 9 10C7.34315 10 6 8.65685 6 7C6 5.34315 7.34315 4 9 4C10.6569 4 12 5.34315 12 7Z';
      case 'search':
        return 'M21 21L16.65 16.65M18 10.5C18 14.6421 14.6421 18 10.5 18C6.35786 18 3 14.6421 3 10.5C3 6.35786 6.35786 3 10.5 3C14.6421 3 18 6.35786 18 10.5Z';
      case 'library':
        return 'M6 4.5H10V20H6V4.5ZM14 4.5H18V20H14V4.5ZM10 7.5H14V20H10V7.5Z';
      case 'loans':
        return 'M7 4H17C18.1046 4 19 4.89543 19 6V20L16 18L13 20L10 18L7 20V6C7 4.89543 7.89543 4 9 4ZM9.5 8H14.5M9.5 11.5H16M9.5 15H13.5';
      case 'profile':
        return 'M12 12C14.7614 12 17 9.76142 17 7C17 4.23858 14.7614 2 12 2C9.23858 2 7 4.23858 7 7C7 9.76142 9.23858 12 12 12ZM4 21C4 17.6863 7.58172 15 12 15C16.4183 15 20 17.6863 20 21';
    }
  }
}
