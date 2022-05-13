import {
  UserFormComponent,
  UserFormModule,
  UserFormModel,
  FormState,
} from './../../shared/components/user-form/user-form.component';
import { UserService } from './../../shared/services/user.service';
import { Component, NgModule, OnInit, ViewChild } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import {
  DxButtonModule,
  DxDataGridModule,
  DxPopupModule,
} from 'devextreme-angular';
import { EnumUserSex, UserModel } from 'src/app/shared/models/user.model';

import { AuthService } from 'src/app/shared/services';
import { PositionService } from 'src/app/shared/services/position.service';
import { PositionModel } from 'src/app/shared/models/position.model';
import { AppRolesEnum } from 'src/app/shared/models/user-app.model';

@Component({
  selector: 'app-user',
  templateUrl: './user.component.html',
  styleUrls: ['./user.component.scss'],
})
export class UserComponent implements OnInit {
  //#region Init variable
  dataSource: UserModel[];
  positionDataSource: PositionModel[];

  isAdminRole = false;
  sexDataSource = [
    { caption: 'Male', value: EnumUserSex.MALE },
    { caption: 'Female', value: EnumUserSex.FEMALE },
  ];
  roleDataSource = [
    { caption: 'ADMINISTRATOR', value: AppRolesEnum.ADMINISTRATOR },
    { caption: 'USER', value: AppRolesEnum.USER },
  ];

  gridColumns: [
    'email',
    'firstName',
    'lastName',
    'position',
    'phoneNo',
    'status'
  ];

  @ViewChild(UserFormComponent) userFormComponent: UserFormComponent;
  currUser: UserFormModel = new UserFormModel();
  //#endregion

  constructor(
    private userService: UserService,
    private authService: AuthService,
    private positionService: PositionService
  ) {
    userService.getUsers('').subscribe(
      (next) => {
        this.dataSource = next;
      },
      (error) => {}
    );

    this.positionService.getPositions().subscribe(
      (next) => {
        this.positionDataSource = next;
      },
      (error) => {}
    );
    this.isAdminRole = this.authService.isRoleAdministrator;
  }

  onToolbarPreparing(e) {
    e.toolbarOptions.items.unshift({
      location: 'after',
      widget: 'dxButton',
      options: {
        icon: 'add',
        width: 'auto',
        text: 'Add',
        stylingMode: 'contained',
        type: 'success',
        visible: this.isAdminRole,
        onClick: this.onOpenAddUserPopup.bind(this),
      },
    });
  }

  //#region POPUP

  onOpenAddUserPopup(): void {
    this.currUser.state = FormState.CREATE;
    this.currUser.data = new UserModel();
    this.currUser.data.firstName = '';
    this.currUser.data.lastName = '';
    this.currUser.data.email = '';

    this.currUser.data.status = false;

    this.userFormComponent.open();
  }

  onOpenDetailButton(e, data): void {
    this.currUser.state = FormState.DETAIL;
    this.currUser.data = new UserModel(data.data);

    this.userFormComponent.open();
  }

  onRefreshGrid() {
    this.userService.getUsers('').subscribe(
      (next) => {
        this.dataSource = next;
      },
      (error) => {}
    );
  }
  //#endregion

  ngOnInit(): void {}
}

@NgModule({
  imports: [
    BrowserModule,
    DxDataGridModule,
    DxButtonModule,
    DxPopupModule,

    UserFormModule,
  ],
  declarations: [UserComponent],
  bootstrap: [UserComponent],
})
export class UserModule {}
