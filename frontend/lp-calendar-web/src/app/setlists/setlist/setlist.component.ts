import {Component, Input, OnInit} from '@angular/core';
import {FormsModule, ReactiveFormsModule} from "@angular/forms";
import {SetlistsService} from '../../services/setlists.service';
import {Router} from '@angular/router';
import {ToastrService} from 'ngx-toastr';
import {ErrorResponseDto, SetlistDto} from '../../modules/lpshows-api';
import {Setlist} from '../../data/setlists/setlist';

@Component({
  selector: 'app-setlist',
    imports: [
        FormsModule,
        ReactiveFormsModule
    ],
  templateUrl: './setlist.component.html',
  styleUrl: './setlist.component.css',
})
export class SetlistComponent implements OnInit {
  @Input({ required: false })
  setlistId: number | undefined;

  @Input({ required: false })
  setlist: Setlist | undefined;

  setlistTitle$: string = "Setlist";

  constructor(private setlistService: SetlistsService, private toastr: ToastrService) {
  }

  private didLoadSetlist() {
    console.debug("Found setlist", this.setlist);
    this.setlistTitle$ = this.setlist?.concertId ?? "Setlist";
  }

  ngOnInit() {
    if (this.setlist == undefined && this.setlistId !== undefined) {
      this.setlistService.getSetlist(this.setlistId).subscribe({
        next: data => {
          this.setlist = Setlist.fromDto(data);
          this.didLoadSetlist();
        },
        error: err => {
          let errorResponse: ErrorResponseDto = err.error;
          this.toastr.error(errorResponse.message, "Could not load setlist");
        }
      })
    }
  }
}
