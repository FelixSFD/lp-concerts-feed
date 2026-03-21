import {Component, inject, OnInit} from '@angular/core';
import {NgbModal} from '@ng-bootstrap/ng-bootstrap';
import {SongsService} from '../../../services/songs.service';
import {ToastrService} from 'ngx-toastr';
import {ErrorResponseDto, SongMashupDto} from '../../../modules/lpshows-api';
import {RouterLink} from '@angular/router';

@Component({
  selector: 'app-manage-mashups-page',
  imports: [
    RouterLink
  ],
  templateUrl: './manage-mashups-page.component.html',
  styleUrl: './manage-mashups-page.component.css',
})
export class ManageMashupsPageComponent implements OnInit {
  private toastr = inject(ToastrService);
  private modalService = inject(NgbModal);
  private songsService = inject(SongsService);


  mashups$: SongMashupDto[] = [];


  ngOnInit() {
    this.reloadList(true);
  }


  private reloadList(cache: boolean) {
    this.songsService.getAllMashups(cache).subscribe({
      next: mashups => {
        this.mashups$ = mashups;
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        this.toastr.error(errorResponse.message, "Could not load mashups!");
      }
    })
  }
}
