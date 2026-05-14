import { Component, Input } from '@angular/core';
import {SetlistEntrySongExtraDto} from '../../../modules/lpshows-api';
import {
  SetlistEntrySongExtraFormContent
} from '../setlist-entry-song-extra-form/setlist-entry-song-extra-form.component';
import {NgTemplateOutlet} from '@angular/common';

@Component({
  selector: 'app-setlist-entry-song-extra-list',
  imports: [
    NgTemplateOutlet
  ],
  templateUrl: './setlist-entry-song-extra-list.component.html',
  styleUrl: './setlist-entry-song-extra-list.component.css',
})
export class SetlistEntrySongExtraListComponent {
  @Input("extras")
  songExtras$: SetlistEntrySongExtraDto[] = [];


  protected readonly SetlistEntrySongExtraFormContent = SetlistEntrySongExtraFormContent;
}
