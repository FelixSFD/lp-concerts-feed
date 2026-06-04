import { Component } from '@angular/core';
import {Card} from 'primeng/card';
import {Divider} from 'primeng/divider';
import {Select} from 'primeng/select';
import {Accordion, AccordionContent, AccordionHeader, AccordionPanel} from 'primeng/accordion';

@Component({
  selector: 'app-about-page',
  imports: [
    Card,
    Divider,
    Select,
    Accordion,
    AccordionPanel,
    AccordionHeader,
    AccordionContent
  ],
  templateUrl: './about-page.component.html',
  styleUrl: './about-page.component.css'
})
export class AboutPageComponent {

}
