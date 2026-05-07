import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-filter-select',
  templateUrl: './filter-select.component.html',
  styleUrl: './filter-select.component.scss'
})
export class FilterSelectComponent {
  readonly ariaLabel = input('Filtrar');
  readonly options = input<readonly string[]>([]);
  readonly value = input('');
  readonly valueChange = output<string>();

  onChange(event: Event): void {
    const target = event.target as HTMLSelectElement;
    this.valueChange.emit(target.value);
  }
}
