import {Component, inject} from '@angular/core';
import {MashupFormComponent, MashupFormContent} from '../mashup-form/mashup-form.component';
import {SongsService} from '../../../../../services/songs.service';
import {CreateSongMashupRequestDto, ErrorResponseDto} from '../../../../../modules/lpshows-api';
import {Router, RouterLink} from '@angular/router';
import {Button} from 'primeng/button';
import {Card} from 'primeng/card';
import {MessageService} from 'primeng/api';

@Component({
  selector: 'app-add-mashup-page',
  imports: [
    MashupFormComponent,
    RouterLink,
    Button,
    Card
  ],
  templateUrl: './add-mashup-page.component.html',
  styleUrl: './add-mashup-page.component.css',
})
export class AddMashupPageComponent {
  private messageService = inject(MessageService);
  private songsService = inject(SongsService);
  private router = inject(Router);

  isAdding$ = false;

  onSaveClicked(formContent: MashupFormContent) {
    this.isAdding$ = true;

    let request: CreateSongMashupRequestDto = {
      title: formContent.title,
      linkinpediaUrl: formContent.linkinpediaUrl,
      songIds: formContent.songs.map(s => s.id!)
    };

    this.songsService.createMashup(request).subscribe({
      next: createdMashup => {
        console.debug('Created new mashup', createdMashup);
        this.isAdding$ = false;
        this.router.navigate(["/", "admin", "mashups", createdMashup.id]).catch(err => {
          this.messageService.add({
            severity: "danger",
            summary: "Failed to navigate to the create mashup",
            text: err.message,
          });
        });
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        this.messageService.add({
          severity: "danger",
          summary: "Could not create mashup",
          text: errorResponse.message,
        });
        this.isAdding$ = false;
      }
    });
  }
}
