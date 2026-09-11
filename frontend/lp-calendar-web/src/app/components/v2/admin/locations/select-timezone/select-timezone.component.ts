import {
  booleanAttribute,
  Component,
  EventEmitter,
  forwardRef,
  Input,
  OnInit,
  Output,
  signal,
} from '@angular/core';
import { ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { Select } from 'primeng/select';
import timezones, { TimeZone } from 'timezones-list';

@Component({
  selector: 'app-select-timezone',
  imports: [
    FormsModule,
    Select,
  ],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SelectTimezoneComponent),
      multi: true,
    },
  ],
  templateUrl: './select-timezone.component.html',
  styleUrl: './select-timezone.component.css',
})
export class SelectTimezoneComponent implements ControlValueAccessor, OnInit {
  @Input() inputId: string = 'timezone';
  @Input() placeholder: string = '';
  @Input({ transform: booleanAttribute }) fluid: boolean = true;
  @Input({ transform: booleanAttribute }) showClear: boolean = false;
  @Input({ transform: booleanAttribute }) filter: boolean = true;
  @Input({ transform: booleanAttribute }) disabled: boolean = false;
  @Input({ transform: booleanAttribute }) invalid: boolean = false;
  @Input() appendTo: any = undefined;

  /**
   * List of timezones that are available for selection. If not provided, the default list from timezones-list is loaded.
   */
  @Input('available-timezones') availableTimezones: TimeZone[] | null | undefined = null;

  @Output() timezoneChange = new EventEmitter<string | null>();

  timezones = signal<TimeZone[]>([]);
  loading = signal(false);
  value = signal<string | null>(null);

  private onChange: (value: string | null) => void = () => {};
  private onTouched: () => void = () => {};

  ngOnInit() {
    this.loadTimezones();
  }

  loadTimezones() {
    if (this.availableTimezones && this.availableTimezones.length > 0) {
      this.timezones.set(this.availableTimezones);
    } else {
      this.timezones.set(timezones ?? []);
    }
  }

  writeValue(value: any): void {
    if (value === null || value === undefined || value === '') {
      this.value.set(null);
    } else {
      this.value.set(String(value));
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
      this.value.set(String(newValue));
    }
    this.onChange(this.value());
    this.onTouched();
    this.timezoneChange.emit(this.value());
  }

  onBlur() {
    this.onTouched();
  }
}
