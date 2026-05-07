import { Component, computed, input } from '@angular/core';
import { AccentTone } from '../../models/user.model';

@Component({
  selector: 'app-status-badge',
  templateUrl: './status-badge.component.html',
  styleUrl: './status-badge.component.scss'
})
export class StatusBadgeComponent {
  readonly label = input.required<string>();
  readonly tone = input<AccentTone>('slate');
  readonly dot = input(false);

  protected readonly className = computed(() => `status-badge--${this.tone()}`);
}
