import { Component, effect, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ProblemDetailsDto, PublicUserDto } from '../../../../modules/lpshows-api/v3';
import { Card } from 'primeng/card';
import { Tag } from 'primeng/tag';
import { Tooltip } from 'primeng/tooltip';

@Component({
  imports: [
    Card,
    Tag,
    Tooltip
  ],
  selector: 'app-user-profile-page',
  styleUrl: './user-profile-page.component.css',
  templateUrl: './user-profile-page.component.html',
})
export class UserProfilePageComponent {
  private readonly route = inject(ActivatedRoute);
  protected username = signal<string>("");
  protected resolverError = signal<ProblemDetailsDto | null>(null);

  private user = signal<PublicUserDto | null>(null);
  private userUpdatedEffect = effect(() => {
    let user = this.user();
    if (user == null) {
      return;
    }

    console.debug("User updated:", user);
    this.username.set(user.username);
  });

  constructor() {
    this.route.data.subscribe(data => {
      console.debug("Resolved data:", data);

      let user = data['user'] as PublicUserDto | null;
      if (user == null || !user.id) {
        this.resolverError.set(data['user'] as ProblemDetailsDto);
        this.user.set(null);
        return;
      }

      this.user.set(user);
    });
  }
}
