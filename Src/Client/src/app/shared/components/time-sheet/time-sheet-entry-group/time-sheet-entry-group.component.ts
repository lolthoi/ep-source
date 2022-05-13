import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import {
  style,
  animate,
  transition,
  trigger,
  state,
} from '@angular/animations';

@Component({
  selector: 'time-sheet-entry-group',
  templateUrl: './time-sheet-entry-group.component.html',
  styleUrls: ['./time-sheet-entry-group.component.scss'],
  animations: [
    trigger('fade', [
      // the "in" style determines the "resting" state of the element when it is visible.
      state('in', style({ opacity: 1 })),

      // fade in when created. this could also be written as transition('void => *')
      transition(':enter', [style({ opacity: 0 }), animate(400)]),

      // fade out when destroyed. this could also be written as transition('void => *')
      transition(':leave', animate(400, style({ opacity: 0 }))),
    ]),
  ],
})
export class TimeSheetEntryGroupComponent implements OnInit {
  @Input() date;
  @Input() entries;
  @Output() onRefreshGrid = new EventEmitter();

  constructor() {}

  ngOnInit(): void {}

  refreshGrid() {
    this.onRefreshGrid.emit();
  }
}
