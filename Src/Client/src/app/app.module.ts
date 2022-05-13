import { PositionService } from './shared/services/position.service';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppComponent } from './app.component';
import {
  SideNavInnerToolbarModule,
  SideNavOuterToolbarModule,
  SingleCardModule,
} from './layouts';
import { FooterModule, LoginFormModule } from './shared/components';
import { AuthService, ScreenService, AppInfoService } from './shared/services';
import { UnauthenticatedContentModule } from './unauthenticated-content';
import { AppRoutingModule } from './app-routing.module';
import { ErrorComponent } from './pages/error/error.component';
import { ApiInterceptor } from './http-interceptor/api-interceptor';
import { JwtHelperService, JwtModule } from '@auth0/angular-jwt';
import { UserEvaluationService } from './shared/services/evaluation-service';
import {
  SocialLoginModule,
  SocialAuthServiceConfig,
  GoogleLoginProvider,
} from 'angularx-social-login';
import { environment } from 'src/environments/environment';
import { NgxPaginationModule } from 'ngx-pagination';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

@NgModule({
  declarations: [AppComponent, ErrorComponent],
  imports: [
    BrowserModule,
    SideNavInnerToolbarModule,
    SingleCardModule,
    FooterModule,
    LoginFormModule,
    UnauthenticatedContentModule,
    AppRoutingModule,
    HttpClientModule,
    NgxPaginationModule,
    SocialLoginModule,
    BrowserAnimationsModule,
    JwtModule.forRoot({
      config: {
        tokenGetter: function tokenGetter() {
          if (
            localStorage.getItem('user') == null ||
            localStorage.getItem('user') == undefined
          ) {
            return 'a';
          }
          return JSON.parse(localStorage.getItem('user'))['token'];
        },
      },
    }),
  ],
  providers: [
    AuthService,
    ScreenService,
    AppInfoService,
    JwtHelperService,
    PositionService,
    UserEvaluationService,
    { provide: HTTP_INTERCEPTORS, useClass: ApiInterceptor, multi: true },
    {
      provide: 'SocialAuthServiceConfig',
      useValue: {
        autoLogin: false,
        providers: [
          {
            id: GoogleLoginProvider.PROVIDER_ID,
            provider: new GoogleLoginProvider(environment.googleClientId),
          },
        ],
      } as SocialAuthServiceConfig,
    },
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}
