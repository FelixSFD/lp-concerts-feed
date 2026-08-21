import {
  booleanAttribute, ChangeDetectionStrategy,
  Component,
  EventEmitter,
  forwardRef,
  inject,
  Input,
  OnChanges,
  OnInit,
  Output,
  SimpleChanges,
} from '@angular/core';
import { ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { Select } from 'primeng/select';
import { ToursService } from '../../../../../services/tours.service';
import { TourDto, TourLegDto } from '../../../../../modules/lpshows-api/v3';

@Component({
  selector: 'app-select-tour-leg',
  imports: [
    FormsModule,
    Select,
  ],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SelectTourLegComponent),
      multi: true,
    },
  ],
  templateUrl: './select-tour-leg.component.html',
  styleUrl: './select-tour-leg.component.css',
  changeDetection: ChangeDetectionStrategy.Eager,
})
export class SelectTourLegComponent implements ControlValueAccessor, OnInit, OnChanges {
  @Input() tour: TourDto | null = null;
  @Input() inputId: string = 'tourLegId';
  @Input() placeholder: string = '';
  @Input({ transform: booleanAttribute }) fluid: boolean = true;
  @Input({ transform: booleanAttribute }) showClear: boolean = false;
  @Input({ transform: booleanAttribute }) filter: boolean = false;
  @Input({ transform: booleanAttribute }) disabled: boolean = false;
  @Input({ transform: booleanAttribute }) invalid: boolean = false;

  @Output() legChange = new EventEmitter<string | null>();

  tourLegs: TourLegDto[] = [];
  loading: boolean = false;
  value: string | null = null;

  private onChange: (value: string | null) => void = () => {};
  private onTouched: () => void = () => {};

  ngOnInit() {
    this.updateTourLegs();
  }

  ngOnChanges(changes: SimpleChanges<SelectTourLegComponent>) {
    console.debug('SelectTourLegComponent ngOnChanges', changes);
    if (changes.tour) {
      console.debug('SelectTourLegComponent ngOnChanges tour changed', changes.tour.currentValue);
      this.updateTourLegs();
    }
  }

  private updateTourLegs() {
    this.tourLegs = this.tour?.legs ?? [];
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
    this.legChange.emit(this.value);
  }

  onBlur() {
    this.onTouched();
  }
}
