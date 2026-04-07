import {Component, inject} from '@angular/core';
import {FormBuilder, FormControl, ReactiveFormsModule, Validators} from '@angular/forms';
import {NgClass} from '@angular/common';
import {HttpClient} from '@angular/common/http';
import {ToastrService} from 'ngx-toastr';
import {SetlistsService} from '../../services/setlists.service';
import {ErrorResponseDto, ImportSetlistPreviewDto} from '../../modules/lpshows-api';

@Component({
  selector: 'app-linkinpedia-concert-importer-page',
  imports: [
    ReactiveFormsModule,
    NgClass
  ],
  templateUrl: './linkinpedia-concert-importer-page.component.html',
  styleUrl: './linkinpedia-concert-importer-page.component.css',
})
export class LinkinpediaConcertImporterPageComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly httpClient = inject(HttpClient);
  private readonly toastr = inject(ToastrService);
  private readonly setlistsService = inject(SetlistsService);

  // true if the page is currently reading the source information
  isReadingSource$ = false;

  tmpLinkinpediaSource$: string | null = null;

  sourceDataForm = this.formBuilder.group({
    linkinpediaUrl: new FormControl('https://linkinpedia.com/wiki/Live:20240905', [Validators.required, Validators.pattern(/^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&\/=]*)/)]),
  });

  generatedSetlist$: ImportSetlistPreviewDto | null = null;


  onLoadSourceClicked() {
    this.startImport();
  }


  private startImport() {
    let url = this.sourceDataForm.value.linkinpediaUrl?.valueOf();
    if (url) {
      this.isReadingSource$ = true;
      this.setlistsService.getImportInfosFromLinkinpedia(url)
        .subscribe({
          next: data => {
            this.generatedSetlist$ = data;

            this.toastr.success('Successfully read setlist from Linkinpedia');
            this.isReadingSource$ = false;
          },
          error: err => {
            let errorResponse: ErrorResponseDto = err.error;
            console.warn("Failed to fetch import data:", err);

            this.toastr.error(errorResponse.message, "Could not get import data!");
            this.isReadingSource$ = false;
          }
        });
    }
  }

  private loadFromLinkinpedia() {
    let url = this.sourceDataForm.value.linkinpediaUrl?.valueOf() ?? null;
    if (url) {
      let apiUrl = this.getApiUrlForPage(url);
      console.debug("Linkinpedia API URL: ", apiUrl);

      this.httpClient.get(apiUrl).subscribe({
        next: data => {
          console.debug("API result: ", data);
        },
        error: error => {
          console.error("API call to Linkinpedia failed: ", error);
          this.toastr.error(error);
        }
      })
    } else {
      this.toastr.error('URL is not set.');
    }
  }


  private getApiUrlForPage(linkinpediaUrl: string): string {
    const regex = /linkinpedia\.com\/(?<originalPath>wiki\/)(?<page>[^\/]+)$/gm;
    const subst = `linkinpedia.com/w/rest.php/v1/page/$2`;
    return linkinpediaUrl.replace(regex, subst);
  }
}
