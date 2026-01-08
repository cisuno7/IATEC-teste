import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

import { DashboardPageComponent } from './pages/dashboard.page';
import { FiltersComponent } from './components/filters/filters.component';
import { ParticipantsComponent } from './components/participants/participants.component';
import { EventModalComponent } from './components/event-modal/event-modal.component';
import { AuthGuard } from '../../core/auth/auth.guard';
import { FormsModule } from '@angular/forms';

@NgModule({
  declarations: [
    DashboardPageComponent,
    FiltersComponent,
    ParticipantsComponent,
    EventModalComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
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
