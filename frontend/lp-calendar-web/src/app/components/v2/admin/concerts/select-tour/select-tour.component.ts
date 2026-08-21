import { booleanAttribute, Component, EventEmitter, forwardRef, inject, Input, OnInit, Output } from '@angular/core';
import { ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { Select } from 'primeng/select';
import { ToursService } from '../../../../../services/tours.service';
import { TourDto } from '../../../../../modules/lpshows-api/v3';

@Component({
  selector: 'app-select-tour',
  imports: [
    FormsModule,
    Select,
  ],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SelectTourComponent),
      multi: true,
    },
  ],
  templateUrl: './select-tour.component.html',
  styleUrl: './select-tour.component.css',
})
export class SelectTourComponent implements ControlValueAccessor, OnInit {
  private toursService = inject(ToursService);

  @Input() inputId: string = 'tourId';
  @Input() placeholder: string = '';
  @Input({ transform: booleanAttribute }) fluid: boolean = true;
  @Input({ transform: booleanAttribute }) showClear: boolean = false;
  @Input({ transform: booleanAttribute }) filter: boolean = false;
  @Input({ transform: booleanAttribute }) disabled: boolean = false;
  @Input({ transform: booleanAttribute }) invalid: boolean = false;

  @Output() tourChange = new EventEmitter<string | null>();

  tours: TourDto[] = [];
  loading: boolean = false;
  value: string | null = null;

  private onChange: (value: string | null) => void = () => {};
  private onTouched: () => void = () => {};

  ngOnInit() {
    this.loadTours();
  }

  loadTours() {
    this.loading = true;
    this.toursService.getTours().subscribe({
      next: (tours: any) => {
        this.tours = Array.isArray(tours) ? tours : tours ? [tours] : [];
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load tours', err);
        this.loading = false;
      },
    });
  }

  writeValue(value: any): void {
    if (value === null || value === undefined || value === '') {
      this.value = null;
    } else {
      this.value = String(value);
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
      this.value = null;
    } else {
      this.value = String(newValue);
    }
    this.onChange(this.value);
    this.onTouched();
    this.tourChange.emit(this.value);
  }

  onBlur() {
    this.onTouched();
  }
}
