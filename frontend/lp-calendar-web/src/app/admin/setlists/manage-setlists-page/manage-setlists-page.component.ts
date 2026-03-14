import {Component, OnInit} from '@angular/core';
import {RouterLink} from '@angular/router';
import {SetlistsService} from '../../../services/setlists.service';
import {ToastrService} from 'ngx-toastr';
import {Setlist} from '../../../data/setlists/setlist';

@Component({
  selector: 'app-manage-setlists-page',
  imports: [
    RouterLink
  ],
  templateUrl: './manage-setlists-page.component.html',
  styleUrl: './manage-setlists-page.component.css',
})
export class ManageSetlistsPageComponent implements OnInit {
  setlists$: Setlist[] = [];

  constructor(private setlistService: SetlistsService, private toastr: ToastrService) {
  }


  ngOnInit() {
    this.setlistService.getSetlists().subscribe(setlists => {
      this.setlists$ = setlists.map(setlist => Setlist.fromDto(setlist));
    })
  }
}
