import {
  booleanAttribute,
  Component,
  EventEmitter,
  forwardRef,
  inject,
  Input,
  OnInit,
  Output,
  signal
} from '@angular/core';
import { ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { Select } from 'primeng/select';
import { ConcertTypesService } from '../../../../../services/concert-types.service';
import { ConcertTypeDto } from '../../../../../modules/lpshows-api/v3';

@Component({
  selector: 'app-select-concert-type',
  imports: [
    FormsModule,
    Select,
  ],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SelectConcertTypeComponent),
      multi: true,
    },
  ],
  templateUrl: './select-concert-type.component.html',
  styleUrl: './select-concert-type.component.css',
})
export class SelectConcertTypeComponent implements ControlValueAccessor, OnInit {
  private concertTypesService = inject(ConcertTypesService);

  @Input() inputId: string = 'concertTypeId';
  @Input() placeholder: string = '';
  @Input({ transform: booleanAttribute }) fluid: boolean = true;
  @Input({ transform: booleanAttribute }) showClear: boolean = false;
  @Input({ transform: booleanAttribute }) filter: boolean = false;
  @Input({ transform: booleanAttribute }) disabled: boolean = false;
  @Input({ transform: booleanAttribute }) invalid: boolean = false;

  @Output() concertTypeChange = new EventEmitter<number | null>();

  concertTypes = signal<ConcertTypeDto[]>([]);
  loading = signal(false);
  value = signal<number | null>(null);

  private onChange: (value: number | null) => void = () => {};
  private onTouched: () => void = () => {};

  ngOnInit() {
    this.loadConcertTypes();
  }

  loadConcertTypes() {
    this.loading();
    this.concertTypesService.getConcertTypes().subscribe({
      next: (types: any) => {
        this.concertTypes.set(Array.isArray(types) ? types : types ? [types] : []);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load concert types', err);
        this.loading.set(false);
      },
    });
  }

  writeValue(value: any): void {
    if (value === null || value === undefined || value === '') {
      this.value.set(null);
    } else {
      this.value.set(Number(value));
    }
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  onValueChange(newValue: any) {
    if (newValue === null || newValue === undefined || newValue === '') {
      this.value.set(null);
    } else {
      this.value.set(Number(newValue));
    }
    this.onChange(this.value());
    this.onTouched();
    this.concertTypeChange.emit(this.value());
  }

  onBlur() {
    this.onTouched();
  }
}
