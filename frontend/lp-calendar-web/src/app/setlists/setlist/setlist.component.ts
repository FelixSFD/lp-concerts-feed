import {Component, Input, OnInit} from '@angular/core';
import {FormsModule, ReactiveFormsModule} from "@angular/forms";
import {SetlistsService} from '../../services/setlists.service';
import {Router} from '@angular/router';
import {ToastrService} from 'ngx-toastr';
import {ErrorResponseDto} from '../../modules/lpshows-api';

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
  @Input()
  setlistId: number | undefined;

  setlistTitle$: string = "Setlist";

  constructor(private setlistService: SetlistsService, private toastr: ToastrService) {
  }

  ngOnInit() {
    this.setlistService.getSetlist(this.setlistId!).subscribe({
      next: data => {
        console.debug("Found setlist", data);
        this.setlistTitle$ = data.concertId ?? "Setlist";
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        this.toastr.error(errorResponse.message, "Could not load setlist");
      }
    })
  }
}
