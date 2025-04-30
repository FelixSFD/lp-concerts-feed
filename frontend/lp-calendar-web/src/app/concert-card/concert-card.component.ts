import {Component, inject, Input, OnInit, TemplateRef} from '@angular/core';
import {CountdownComponent} from '../countdown/countdown.component';
import {Concert} from '../data/concert';
import {ConcertsService} from '../services/concerts.service';
import {NgIf} from '@angular/common';
import {RouterLink} from '@angular/router';
import {ConcertTitleGenerator} from '../data/concert-title-generator';
import * as htmlToImage from 'html-to-image';
import {NgbModal} from '@ng-bootstrap/ng-bootstrap';
import download from 'downloadjs';

@Component({
  selector: 'app-concert-card',
  imports: [
    CountdownComponent,
    NgIf,
    RouterLink
  ],
  templateUrl: './concert-card.component.html',
  styleUrl: './concert-card.component.css',
  standalone: true
})
export class ConcertCardComponent implements OnInit{
  // how the concert is displayed. "countdown" is default
  displayType: string = "countdown";

  @Input("concert")
  concert$: Concert | null = null;

  private modalService = inject(NgbModal);

  // if no concert is set, use distant past as placeholder for countdown
  pastPlaceholderDate = new Date(2024, 9, 5, 15, 0).toISOString();


  constructor(private concertsService: ConcertsService) {
  }


  ngOnInit(): void {
  }


  openModal(content: TemplateRef<any>) {
    return this.modalService.open(content, { ariaLabelledBy: 'modal-basic-title' });
  }


  shareImage(content: TemplateRef<any>) {
    console.log("shareImage", content);
    this.openModal(content);
  }


  onShareRequested() {
    console.log("onShareRequested");
  }


  generateImage(){
    htmlToImage
      .toPng(document.getElementById('shareCountdownModalBody')!)
      .then((dataUrl) => download(dataUrl, 'my-node.png'));
  }

  protected readonly ConcertTitleGenerator = ConcertTitleGenerator;
  protected readonly alert = alert;
}
