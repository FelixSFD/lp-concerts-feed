import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ConfirmationService } from 'primeng/api';
import { SetlistsService } from '../../../../../services/setlists.service';
import { Setlist } from '../../../../../data/setlists/setlist';
import { Card } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { Button } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';
import { ButtonGroup } from 'primeng/buttongroup';
import { MessageService } from 'primeng/api';
import { FormsModule } from '@angular/forms';
import { ConfirmDialog } from 'primeng/confirmdialog';

const SetlistErrorType = {
  NoConcert: "No concert specified",
  CouldNotLoadSetlists: "Could not load setlists"
};

@Component({
  selector: 'app-manage-setlists-page',
  imports: [
    RouterLink,
    Card,
    TableModule,
    Button,
    InputText,
    IconField,
    InputIcon,
    ButtonGroup,
    FormsModule,
    ConfirmDialog
  ],
  templateUrl: './manage-setlists-page.component.html',
  styleUrls: ['./manage-setlists-page.component.css'],
})
export class ManageSetlistsPageComponent implements OnInit {
  private readonly setlistsService = inject(SetlistsService);
  private messageService = inject(MessageService);

  // Use signals for reactive state management
  setlists = signal<Setlist[]>([]);

  // property to show whether the setlist is currently being deleted
  setlistDeleting = signal(false);

  // true while data is being loaded
  isLoading = signal(false);

  globalSearchText = signal("");

  ngOnInit(): void {
    this.reloadList(true).then();
  }

  private async reloadList(cache: boolean): Promise<void> {
    const cached = cache || false;

    try {
      this.isLoading.set(true);

      // Get headers of all setlists from API
      const headers = await this.setlistsService.getSetlistHeaders();

      if (headers.length === 0) {
        this.messageService.add({
          severity: 'info',
          summary: 'No setlists available',
          text: SetlistErrorType.NoConcert,
          sticky: true
        });
        this.isLoading.set(false);
        return;
      }

      // Get details for each setlist header
      const promises = headers.map(async (header) => {
        try {
          const dto = await this.setlistsService.getCompleteSetlist(header.id!);
          return Setlist.fromDto(dto);
        } catch (error: any) {
          console.warn(`Failed to load setlist ${header.id}:`, error);
          return null;
        }
      });

      const loadedSetlists = await Promise.all(promises);
      const filteredSetlists = loadedSetlists.filter(s => s !== null);

      // Update the signal with new setlist data
      this.setlists.set(filteredSetlists);
      this.isLoading.set(false);
    } catch (err: any) {
      this.isLoading.set(false);

      if (Array.isArray(err)) {
        const firstError = err[0];
        if (firstError?.message === SetlistErrorType.NoConcert) {
          this.messageService.add({ severity: 'error', summary: SetlistErrorType.NoConcert, text: SetlistErrorType.NoConcert });
        } else if (firstError?.message === SetlistErrorType.CouldNotLoadSetlists) {
          this.messageService.add({ severity: 'error', summary: SetlistErrorType.CouldNotLoadSetlists, text: SetlistErrorType.CouldNotLoadSetlists });
        } else if (firstError?.message === 'Cannot read properties of undefined') {
          this.messageService.add({ severity: 'error', summary: SetlistErrorType.NoConcert, text: SetlistErrorType.NoConcert });
        } else if (firstError?.message) {
          this.messageService.add({ severity: 'error', summary: firstError.title || firstError.message, text: firstError.message || '' });
        } else if (firstError?.status === 404) {
          this.messageService.add({ severity: 'warn', summary: 'Not Found', text: firstError.detail || '' });
        }
      } else {
        this.messageService.add({ severity: 'error', summary: 'Could not load setlists!', text: err instanceof Error ? err.toString() || String(err) : String(err) });
      }

      // Clear setlists on error
      this.setlists.set([]);
    }
  }

  onDeleteSetlistClicked(event: Event, setlist: Setlist): void {
    this.confirmationService.confirm({
      target: event.target as EventTarget,
      message: `Do you want to delete the setlist for "${setlist.concertTitle}"?`,
      header: 'Delete Setlist',
      icon: 'pi pi-info-circle',
      rejectLabel: 'Cancel',
      rejectButtonProps: { label: 'Cancel', severity: 'secondary', outlined: true },
      acceptButtonProps: { label: 'Delete', severity: 'danger' },
      accept: async () => await this.onDeleteSetlistConfirm(setlist),
    });
  }

  async onDeleteSetlistConfirm(setlist: Setlist): Promise<void> {
    const id = setlist.id;
    console.debug("Will delete setlist: " + id);

    try {
      const result = await this.setlistsService.deleteSetlist(id);
      console.debug("DELETE setlist request finished");
      console.debug(result);
      await this.reloadList(false);
      this.setlistDeleting.set(false);
    } catch (err) {
      if (Array.isArray(err) && err.length > 0) {
        const wrappedErr = err[0];
        if (wrappedErr?.status === 404) {
          this.messageService.add({severity: 'warn', summary: 'Setlist not found', text: wrappedErr.detail || ''});
          this.setlistDeleting.set(false);
          return Promise.resolve();
        } else if (wrappedErr?.message) {
          this.messageService.add({
            severity: 'error',
            summary: wrappedErr.title || wrappedErr.message,
            text: wrappedErr.detail || wrappedErr.message
          });
          this.setlistDeleting.set(false);
        }
      }

      const errText = typeof err === 'string' ? err : (err instanceof Error ? err.toString() : JSON.stringify(err));
      console.warn('Failed to delete setlist:', err);
      this.messageService.add({severity: 'error', summary: 'Could not delete setlist!', text: errText});
      this.setlistDeleting.set(false);
      return await Promise.reject(err);
    }
  }

  private confirmationService = inject(ConfirmationService);
}
