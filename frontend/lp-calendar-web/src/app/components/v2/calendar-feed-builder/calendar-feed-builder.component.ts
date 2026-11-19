import {Component, DestroyRef, EventEmitter, inject, OnInit, Output, signal} from '@angular/core';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';
import {FormBuilder, ReactiveFormsModule} from '@angular/forms';
import {ToggleSwitch} from 'primeng/toggleswitch';
import {CalendarSubscriptionSettingsDto} from '../../../modules/lpshows-api/v3';

/** Collects the event-time settings for a calendar subscription. */
@Component({
  selector: 'app-calendar-feed-builder',
  imports: [
    ReactiveFormsModule,
    ToggleSwitch,
  ],
  templateUrl: './calendar-feed-builder.component.html',
  styleUrl: './calendar-feed-builder.component.css'
})
export class CalendarFeedBuilderComponent implements OnInit {
  @Output() settingsChange = new EventEmitter<CalendarSubscriptionSettingsDto>();
  private readonly formBuilder = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  protected readonly feedBuilderForm = signal(this.formBuilder.nonNullable.group({
    includeDoorsTime: false,
    includeLpStageTime: true,
  }));

  /** Emits the initial settings and subsequent changes. */
  ngOnInit() {
    this.settingsChange.emit(this.feedBuilderForm().getRawValue());
    this.feedBuilderForm().valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.settingsChange.emit(this.feedBuilderForm().getRawValue());
    });
  }
}
