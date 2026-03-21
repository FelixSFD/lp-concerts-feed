import {Component, Input} from '@angular/core';
import {NgbTooltip} from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-setlist-entry-icons',
  imports: [
    NgbTooltip
  ],
  templateUrl: './setlist-entry-icons.component.html',
  styleUrl: './setlist-entry-icons.component.css',
})
export class SetlistEntryIconsComponent {
  @Input("record-tape")
  showRecordTape$ = false;

  @Input("rotation-song")
  showRotationSymbol$ = false;

  @Input("world-premiere")
  showGlobeSymbol$ = false;

  @Input("live-premiere")
  showLivePremiereSymbol$ = false;
}
