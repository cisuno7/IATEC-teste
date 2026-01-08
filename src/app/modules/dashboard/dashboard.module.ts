import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

import { DashboardPageComponent } from './pages/dashboard.page';
import { FiltersComponent } from './components/filters/filters.component';
import { ParticipantsComponent } from './components/participants/participants.component';
import { AuthGuard } from '../../core/auth/auth.guard';

@NgModule({
  declarations: [
    DashboardPageComponent,
    FiltersComponent,
    ParticipantsComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule.forChild([
      {
        path: '',
        component: DashboardPageComponent,
        canActivate: [AuthGuard]
      }
    ])
  ]
})
export class DashboardModule { }
