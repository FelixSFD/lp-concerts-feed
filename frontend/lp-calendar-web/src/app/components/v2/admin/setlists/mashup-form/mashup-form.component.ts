import {Component, EventEmitter, inject, Input, OnInit, Output} from '@angular/core';
import {FormBuilder, FormControl, FormsModule, ReactiveFormsModule, Validators} from "@angular/forms";
import {ErrorResponseDto, SongDto, SongMashupDto} from '../../../../../modules/lpshows-api';
import {NgTemplateOutlet} from '@angular/common';
import {SelectSongComponent} from '../select-song/select-song.component';
import {SongsService} from '../../../../../services/songs.service';
import {Button} from 'primeng/button';
import {Card} from 'primeng/card';
import {Divider} from 'primeng/divider';
import {FloatLabel} from 'primeng/floatlabel';
import {InputGroup} from 'primeng/inputgroup';
import {InputGroupAddon} from 'primeng/inputgroupaddon';
import {InputText} from 'primeng/inputtext';
import {TableModule} from 'primeng/table';
import {ButtonGroup} from 'primeng/buttongroup';
import {MessageService} from 'primeng/api';
import {Dialog} from 'primeng/dialog';

@Component({
  selector: 'app-mashup-form',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    SelectSongComponent,
    NgTemplateOutlet,
    Button,
    Card,
    Divider,
    FloatLabel,
    InputGroup,
    InputGroupAddon,
    InputText,
    TableModule,
    ButtonGroup,
    Dialog
  ],
  templateUrl: './mashup-form.component.html',
  styleUrl: './mashup-form.component.css',
})
export class MashupFormComponent implements OnInit {
  private messageService = inject(MessageService);
  private formBuilder = inject(FormBuilder);
  private songsService = inject(SongsService);

  @Input("is-saving")
  isSaving$: boolean = false;

  /*
   * true, if the for is "standalone", meaning it manages its own layout and has a save-button
   */
  @Input("standalone")
  standalone$: boolean = true;

  @Output("saveClicked")
  saveClicked = new EventEmitter<MashupFormContent>();

  mashupForm = this.formBuilder.group({
    title: new FormControl('', [Validators.required]),
    linkinpediaUrl: new FormControl('', [Validators.pattern(/^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&\/=]*)/)]),
  });

  songsInMashup$: SongDto[] = [];

  availableSongs$: SongDto[] = [];

  selectedNewSong$: SongDto | null | undefined;

  isShowingAddSongModal$: boolean = false;


  ngOnInit() {
    this.songsService.getAllSongs(true).subscribe({
      next: data => {
        this.availableSongs$ = data;
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        this.messageService.add({
          severity: 'error',
          summary: 'Could not load the available songs',
          text: errorResponse.message,
        });
      }
    })
  }


  onAddSongClicked() {
    this.isShowingAddSongModal$ = true;
  }


  openLinkinpediaUrlClicked() {
    let url = this.mashupForm.value.linkinpediaUrl?.valueOf();
    if (url?.length == 0) {
      return;
    }

    window.open(url, "_blank");
  }


  onSaveClicked() {
    let content = this.readFromForm();
    if (content) {
      this.saveClicked.emit(content!);
    }
  }


  onSongSelected(song: SongDto) {
    console.debug("onSongSelected", song);
    this.selectedNewSong$ = song;
  }


  onConfirmSongSelectionClicked() {
    if (this.selectedNewSong$) {
      this.songsInMashup$.push(this.selectedNewSong$);
    }

    this.isShowingAddSongModal$ = false;
  }


  onRemoveSongFromMashupClicked(id: number) {
    this.songsInMashup$ = this.songsInMashup$.filter(song => song.id !== id);
  }


  onEditSongClicked(id: number) {
    window.open(`/admin/songs/${id.toString()}`, '_blank');
  }


  public readFromForm(): MashupFormContent | null {
    let title = this.mashupForm.value.title?.valueOf();
    let linkinpediaUrl = this.mashupForm.value.linkinpediaUrl?.valueOf();

    if (title == undefined) {
      this.messageService.add({
        severity: 'error',
        summary: 'Title is required"',
      });
      return null;
    }

    return {
      title: title!,
      linkinpediaUrl: linkinpediaUrl ?? null,
      songs: this.songsInMashup$
    };
  }


  public fillFormWith(mashup: SongMashupDto) {
    console.debug("Fill form with data:", mashup);
    this.mashupForm.controls.title.setValue(mashup.title ?? null);
    this.mashupForm.controls.linkinpediaUrl.setValue(mashup.linkinpediaUrl ?? null);

    this.songsInMashup$ = mashup.songs ?? [];
  }
}


export class MashupFormContent {
  title!: string;
  linkinpediaUrl: string | null = null;
  songs!: SongDto[];
}
