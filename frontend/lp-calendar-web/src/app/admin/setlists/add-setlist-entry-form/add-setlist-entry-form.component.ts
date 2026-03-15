import {Component, inject, OnInit} from '@angular/core';
import {FormBuilder, FormControl, ReactiveFormsModule, Validators} from "@angular/forms";
import {SongsService} from '../../../services/songs.service';
import {ErrorResponseDto, SongDto, SongVariantDto} from '../../../modules/lpshows-api';
import {ToastrService} from 'ngx-toastr';
import {NgClass} from '@angular/common';

@Component({
  selector: 'app-add-setlist-entry-form',
  imports: [
    ReactiveFormsModule,
    NgClass
  ],
  templateUrl: './add-setlist-entry-form.component.html',
  styleUrl: './add-setlist-entry-form.component.css',
})
export class AddSetlistEntryFormComponent implements OnInit {
  private toastr = inject(ToastrService);
  private formBuilder = inject(FormBuilder);
  private songService = inject(SongsService);

  setlistEntryForm = this.formBuilder.group({
    selectedSongId: new FormControl('', []),
    selectedSongVariantId: new FormControl(0, []),
    songVariantName: new FormControl('', []),
    songVariantDescription: new FormControl('', []),
  });


  availableSongs$: SongDto[] = [];

  variantsOfSelectedSong$: SongVariantDto[] = [];

  showAddNewVariantFields: boolean = false;


  ngOnInit(): void {
    this.songService
      .getAllSongs(true)
      .subscribe({
        next: data => {
          this.availableSongs$ = data;
        },
        error: err => {
          let errorResponse: ErrorResponseDto = err.error;
          this.toastr.error(errorResponse.message, "Could not load songs");
        }
      });
  }


  onSongSelectionChanged() {
    this.loadVariantsOfSelectedSong();
  }


  onSongVariantSelectionChanged() {
    let songVariantId = Number(this.setlistEntryForm.value.selectedSongVariantId?.valueOf());
    console.debug("Variant selected: ", songVariantId);

    this.showAddNewVariantFields = songVariantId == -1;
  }


  private loadVariantsOfSelectedSong() {
    console.debug("Load Variants of selectedSong");
    this.variantsOfSelectedSong$ = [];

    let songId = Number(this.setlistEntryForm.value.selectedSongId?.valueOf());
    this.songService.getVariantsOfSong(songId)
      .subscribe(variants => {
        this.variantsOfSelectedSong$ = variants;
      });
  }
}
