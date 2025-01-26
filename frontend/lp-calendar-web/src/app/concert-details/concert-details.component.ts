import {Component, OnDestroy, OnInit} from '@angular/core';
import {ConcertsService} from '../services/concerts.service';
import {Concert} from '../data/concert';
import {ActivatedRoute, RouterLink} from '@angular/router';
import {DateTime} from 'luxon';
import {NgIf} from '@angular/common';
import {CountdownComponent} from '../countdown/countdown.component';
import {Meta} from '@angular/platform-browser';

@Component({
  selector: 'app-concert-details',
  imports: [
    NgIf,
    CountdownComponent,
    RouterLink
  ],
  templateUrl: './concert-details.component.html',
  styleUrl: './concert-details.component.css'
})
export class ConcertDetailsComponent implements OnInit {
  concert$: Concert | null = null;
  concertId: string | undefined;

  constructor(private route: ActivatedRoute, private concertsService: ConcertsService, private metaService: Meta) {
  }

  ngOnInit(): void {
    this.concertId = this.route.snapshot.paramMap.get('id')!;

    this.concertsService
      .getConcert(this.concertId)
      .subscribe(result => {
        if (result != undefined) {
          let concertDateTitleExtension = "";
          if (result.postedStartTime != undefined) {
            let concertDate = new Date(result.postedStartTime);
            concertDateTitleExtension = " - " + concertDate.toLocaleDateString();
          }

          let titleInfo = result.city + ", " + result.country + concertDateTitleExtension;
          window.document.title = window.document.title.replace("Details", titleInfo);

          this.updateMetaInfo(result);
        }

        return this.concert$ = result;
      });
  }


  private updateMetaInfo(concert: Concert) {
    let pageTitle = "";
    let description = "";

    let concertDateDescriptionExtension = "";
    if (concert.postedStartTime != undefined) {
      let concertDate = new Date(concert.postedStartTime);
      concertDateDescriptionExtension = concertDate.toLocaleDateString() + ": ";
    }

    if (concert.tourName != undefined) {
      pageTitle = concert.tourName + ": " + concert.city;
      description = concertDateDescriptionExtension + "Linkin Park show of the " + concert.tourName + " in " + concert.city + ", " + concert.country
    } else {
      pageTitle = "Linkin Park at " + concert.venue;
      description = concertDateDescriptionExtension + "Linkin Park show at " + concert.venue + " in " + concert.city + ", " + concert.country
    }

    this.metaService.updateTag({
      name: "title",
      content: pageTitle
    });
    this.metaService.updateTag({
      property: "og:title",
      content: pageTitle
    });
    this.metaService.updateTag({
      name: "description",
      content: description
    });
    this.metaService.updateTag({
      property: "og:description",
      content: description
    });
  }


  public getDateTime(inputDate: string) {
    return DateTime.fromISO(inputDate, {setZone: false});
  }


  public getDateTimeInTimezone(inputDate: string) {
    return DateTime.fromISO(inputDate, {setZone: true});
  }


  protected readonly DateTime = DateTime;
}
