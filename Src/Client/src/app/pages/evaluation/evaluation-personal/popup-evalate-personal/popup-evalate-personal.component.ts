import {
  Component,
  Input,
  NgModule,
  OnInit,
  Output,
  ViewChild,
} from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { DxPopupModule, DxScrollViewModule } from 'devextreme-angular';
import * as EventEmitter from 'events';
import { $ } from 'protractor';
import { EvaluationDetailModule } from 'src/app/shared/components/evaluation-detail/evaluation-detail.component';
import {
  EvaluationUserDetailComponent,
  EvaluationUserDetailModule,
} from 'src/app/shared/components/evaluation-user-detail/evaluation-user-detail.component';

@Component({
  selector: 'popup-evalate-personal',
  templateUrl: './popup-evalate-personal.component.html',
  styleUrls: ['./popup-evalate-personal.component.scss'],
})
export class PopupEvaluatePersonalComponent implements OnInit {
  @ViewChild(EvaluationUserDetailComponent) userFormComponent;

  popupVisible: boolean = false;
  buttonSubmitVisible: boolean = false;
  submitButtonComponent: any = null;

  constructor() {}

  ngOnInit(): void {}

  openPopUp() {
    this.userFormComponent.onInitSelfEvaluate();
    this.popupVisible = true;
  }

  showSaveButton(value: boolean) {
    this.submitButtonComponent.option('visible', value);
  }

  //#region Options
  closeButtonOptions = {
    text: 'Cancel',
    icon: 'close',
    onClick: (e) => {
      this.popupVisible = false;
      this.userFormComponent.onCancelPopupPersonalEvaluation();
    },
  };

  saveButtonOptions = {
    icon: 'save',
    text: 'Save',
    visible: this.buttonSubmitVisible,
    onInitialized: (e) => {
      this.submitButtonComponent = e.component;
    },
    onClick: (e) => {
      this.userFormComponent.onSubmitPersonalEvaluation();
    },
  };

  onSuccessSavePersonalEvaluate() {
    this.popupVisible = false;
  }

  onHiddenPopup(e) {
    this.userFormComponent.onCancelPopupPersonalEvaluation();
  }
  //
}

@NgModule({
  imports: [
    BrowserModule,
    DxPopupModule,
    EvaluationUserDetailModule,
    DxScrollViewModule,
  ],
  declarations: [PopupEvaluatePersonalComponent],
  bootstrap: [PopupEvaluatePersonalComponent],
  exports: [PopupEvaluatePersonalComponent],
})
export class PopupEvaluatePersonalModule {}
