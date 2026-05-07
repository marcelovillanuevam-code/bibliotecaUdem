import { Component, input, output } from '@angular/core';
import { CurrentUser } from '../../../shared/models/dashboard.model';

@Component({
  selector: 'app-topbar',
  templateUrl: './topbar.component.html',
  styleUrl: './topbar.component.scss'
})
export class TopbarComponent {
  readonly title = input.required<string>();
  readonly user = input.required<CurrentUser>();
  readonly menuToggle = output<void>();
}
