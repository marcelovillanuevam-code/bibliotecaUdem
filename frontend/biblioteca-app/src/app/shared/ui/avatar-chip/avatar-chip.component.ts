import { Component, input } from '@angular/core';
import { AccentTone } from '../../models/user.model';

@Component({
  selector: 'app-avatar-chip',
  templateUrl: './avatar-chip.component.html',
  styleUrl: './avatar-chip.component.scss'
})
export class AvatarChipComponent {
  readonly initials = input.required<string>();
  readonly title = input.required<string>();
  readonly subtitle = input('');
  readonly tone = input<AccentTone>('blue');
  readonly compact = input(false);
}
