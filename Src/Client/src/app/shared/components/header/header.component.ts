import { Component, NgModule, Input, Output, EventEmitter, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';

import { AuthService } from '../../services';
import { UserPanelModule } from '../user-panel/user-panel.component';
import { DxButtonModule } from 'devextreme-angular/ui/button';
import { DxToolbarModule } from 'devextreme-angular/ui/toolbar';

import { Router } from '@angular/router';
import { ChangePasswordFormModule, ChangePasswordFormComponent } from '../change-password-form/change-password-form.component';
@Component({
  selector: 'app-header',
  templateUrl: 'header.component.html',
  styleUrls: ['./header.component.scss']
})

export class HeaderComponent implements OnInit {
  @Output()
  menuToggle = new EventEmitter<boolean>();

  @Input()
  menuToggleEnabled = false;

  @Input()
  title: string;

  @ViewChild(ChangePasswordFormComponent) changePasswordFormComponent: ChangePasswordFormComponent

  user = { email: '' };

  userMenuItems = [{
    text: 'Change Password',
    icon: 'user',
    onClick: () => {
      this.changePasswordFormComponent.open();
    }
  },
  {
    text: 'Log Out',
    icon: 'runner',
    onClick: () => {
      this.authService.logOut();
    }
  }];

  constructor(private authService: AuthService, private router: Router) { }

  ngOnInit() {
    // this.authService.getUser().then((e) => this.user = e.data);
    this.user = this.authService.getUser;
  }

  toggleMenu = () => {
    this.menuToggle.emit();
  }
}

@NgModule({
  imports: [
    CommonModule,
    DxButtonModule,
    UserPanelModule,
    DxToolbarModule,

    ChangePasswordFormModule
  ],
  declarations: [ HeaderComponent ],
  exports: [ HeaderComponent ]
})
export class HeaderModule { }
