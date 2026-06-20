import {Component, EventEmitter, inject, OnInit, Output} from '@angular/core';
import {environment} from '../../../../environments/environment';
import {FormBuilder, FormControl, FormsModule, ReactiveFormsModule} from '@angular/forms';
import {ToggleSwitch} from 'primeng/toggleswitch';

@Component({
  selector: 'app-calendar-feed-builder',
  imports: [
    ReactiveFormsModule,
    ToggleSwitch,
    FormsModule,
  ],
  templateUrl: './calendar-feed-builder.component.html',
  styleUrl: './calendar-feed-builder.component.css'
})
export class CalendarFeedBuilderComponent implements OnInit {
  @Output("feed-url")
  feedUrl = new EventEmitter<string>();


  private formBuilder = inject(FormBuilder);

  feedBuilderForm = this.formBuilder.group({
    doorsTimeSwitch: new FormControl(false, []),
    lpStageTimeSwitch: new FormControl(true, []),
  });


  ngOnInit() {
    this.onSelectionChanged();
    this.feedBuilderForm.valueChanges.subscribe(() => {
      this.onSelectionChanged();
    })
  }


  onSelectionChanged() {
    let url = environment.apiBaseUrlLatest + "/feed/ical"

    let eventCategories: string[] = [];
    if (this.feedBuilderForm.value.doorsTimeSwitch)
      eventCategories.push("Doors");

    if (this.feedBuilderForm.value.lpStageTimeSwitch)
      eventCategories.push("LinkinPark");

    if (eventCategories.length > 0) {
      url += "?event_categories=" + eventCategories.join(",");
    }

    this.feedUrl.emit(url);
  }
}
