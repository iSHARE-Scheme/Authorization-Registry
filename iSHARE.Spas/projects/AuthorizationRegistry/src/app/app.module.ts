import { JwtModule, JWT_OPTIONS } from '@auth0/angular-jwt';
import { FormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import {
  GenericModule,
  AccountModule,
  constants,
  EnvironmentModel,
  LandingPage,
  StorageKeysModel
} from 'common';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { environmentFactory } from '../environment';
import { DefaultLandingPage } from './default-landing-page';
import { storageKeys } from './constants';

export function jwtOptionsFactory(environment: EnvironmentModel) {
  return {
    tokenGetter: () => {
      const value = '; ' + document.cookie;
      const parts = value.split('; ' + storageKeys.auth + '=');
      return parts
        .pop()
        .split(';')
        .shift();
    },
    whitelistedDomains: ['localhost:5000', environment.apiDomain]
  };
}

@NgModule({
  declarations: [AppComponent],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    CommonModule,
    FormsModule,
    AccountModule,
    GenericModule.forRoot(),
    // tslint:disable-next-line:max-line-length
    JwtModule.forRoot({
      jwtOptionsProvider: {
        provide: JWT_OPTIONS,
        useFactory: jwtOptionsFactory,
        deps: [EnvironmentModel]
      }
    })
  ],
  providers: [
    {
      provide: EnvironmentModel,
      useFactory: environmentFactory
    },
    { provide: LandingPage, useClass: DefaultLandingPage },
    { provide: StorageKeysModel, useValue: storageKeys }
  ],
  bootstrap: [AppComponent]
})
export class AppModule {}
