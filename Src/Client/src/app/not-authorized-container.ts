import { CommonModule } from '@angular/common';
import { Component, NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SingleCardModule } from 'src/app/layouts';
import { Router } from '@angular/router';

@Component({
  selector: 'app-not-authorized-container',
  template: `
    <app-single-card [title]="title" [description]="description">
      <router-outlet></router-outlet>
    </app-single-card>
  `,
  styles: [`
    :host {
      width: 100%;
      height: 100%;
    }
  `]
})
export class NotAuthorizedContainerComponent {

  constructor(private router: Router) { }

  get title() {
    const path = this.router.url.split('/')[1];
    switch (path) {
      case 'login-form': return 'Sign In';
      case 'forgot-password': return 'Forgot Password';
      case 'reset-password': return 'Reset Password';
      case 'create-account': return 'Sign Up';
    }
  }

  get description() {
    const path = this.router.url.split('/')[1];
    switch (path) {
      case 'forgot-password': return ;
    }
  }
}
@NgModule({
  imports: [
    CommonModule,
    RouterModule,
    SingleCardModule,
  ],
  declarations: [NotAuthorizedContainerComponent],
  exports: [NotAuthorizedContainerComponent]
})
export class NotAuthorizedContainerModule { }
