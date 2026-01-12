import { Component, EventEmitter, Output } from '@angular/core';
import { FormBuilder, FormGroup, AbstractControl, ValidationErrors } from '@angular/forms';
import { EventFilters } from '../../../../core/services/event.service';

@Component({
  selector: 'app-filters',
  template: `
    <div class="filters-container">
      <div class="filters-header">
        <h3>Filtros</h3>
        <button type="button" class="clear-button" (click)="clearFilters()">
          Limpar Filtros
        </button>
      </div>

      <div class="period-buttons">
        <button
          type="button"
          class="period-button"
          [class.active]="selectedPeriod === 'today'"
          (click)="selectPeriod('today')">
          Dia
        </button>
        <button
          type="button"
          class="period-button"
          [class.active]="selectedPeriod === 'week'"
          (click)="selectPeriod('week')">
          Semana
        </button>
        <button
          type="button"
          class="period-button"
          [class.active]="selectedPeriod === 'month'"
          (click)="selectPeriod('month')">
          Mês
        </button>
      </div>

      <form [formGroup]="filtersForm" class="filters-form">
        <div class="filter-row">
          <div class="filter-group">
            <label class="filter-label">Data Inicial</label>
            <input
              type="date"
              formControlName="startDate"
              class="filter-input"
              [class.error]="isDateRangeInvalid()"
            >
          </div>

          <div class="filter-group">
            <label class="filter-label">Hora Inicial</label>
            <input
              type="time"
              formControlName="startTime"
              class="filter-input"
              [class.error]="isDateRangeInvalid()"
            >
          </div>

          <div class="filter-group">
            <label class="filter-label">Data Final</label>
            <input
              type="date"
              formControlName="endDate"
              class="filter-input"
              [class.error]="isDateRangeInvalid()"
            >
          </div>

          <div class="filter-group">
            <label class="filter-label">Hora Final</label>
            <input
              type="time"
              formControlName="endTime"
              class="filter-input"
              [class.error]="isDateRangeInvalid()"
            >
          </div>
        </div>

        <div class="error-message" *ngIf="isDateRangeInvalid()">
          Data final deve ser maior ou igual à data inicial
        </div>

        <div class="filter-row">
          <div class="filter-group search-group">
            <label class="filter-label">Buscar</label>
            <input
              type="text"
              formControlName="searchText"
              placeholder="Nome, descrição, local, criador ou participantes..."
              class="filter-input search-input"
            >
          </div>
        </div>
      </form>
    </div>
  `,
  styles: [`
    .filters-container {
      background: linear-gradient(135deg, #ffffff 0%, #f8f9fa 100%);
      border-radius: 16px;
      padding: 28px;
      box-shadow: 0 8px 24px rgba(0, 0, 0, 0.08);
      margin-bottom: 32px;
      border: 1px solid #e9ecef;
    }

    .filters-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 24px;
    }

    .filters-header h3 {
      margin: 0;
      color: #1a1a1a;
      font-size: 20px;
      font-weight: 700;
      letter-spacing: -0.02em;
    }

    .clear-button {
      background: #f8f9fa;
      color: #495057;
      border: 2px solid #dee2e6;
      padding: 10px 20px;
      border-radius: 10px;
      cursor: pointer;
      font-size: 14px;
      font-weight: 600;
      transition: all 0.2s ease;
    }

    .clear-button:hover {
      background: #e9ecef;
      border-color: #adb5bd;
      transform: translateY(-1px);
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    }

    .period-buttons {
      display: flex;
      gap: 12px;
      margin-bottom: 24px;
      padding-bottom: 24px;
      border-bottom: 2px solid #e9ecef;
    }

    .period-button {
      flex: 1;
      padding: 14px 24px;
      border: 2px solid #e1e5e9;
      border-radius: 12px;
      background: white;
      color: #495057;
      font-size: 15px;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.3s ease;
    }

    .period-button:hover {
      border-color: #667eea;
      color: #667eea;
      transform: translateY(-2px);
      box-shadow: 0 4px 12px rgba(102, 126, 234, 0.15);
    }

    .period-button.active {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      border-color: #667eea;
      color: white;
      box-shadow: 0 4px 16px rgba(102, 126, 234, 0.3);
    }

    .period-button.active:hover {
      transform: translateY(-2px);
      box-shadow: 0 6px 20px rgba(102, 126, 234, 0.4);
    }

    .filters-form {
      display: flex;
      flex-direction: column;
      gap: 20px;
    }

    .filter-row {
      display: flex;
      gap: 16px;
      flex-wrap: wrap;
    }

    .filter-group {
      display: flex;
      flex-direction: column;
      min-width: 180px;
      flex: 1;
    }

    .search-group {
      flex: 2;
      min-width: 320px;
    }

    .filter-label {
      margin-bottom: 8px;
      color: #495057;
      font-weight: 600;
      font-size: 14px;
      letter-spacing: -0.01em;
    }

    .filter-input {
      padding: 12px 16px;
      border: 2px solid #e1e5e9;
      border-radius: 10px;
      font-size: 14px;
      transition: all 0.3s ease;
      background: white;
      color: #1a1a1a;
    }

    .filter-input:focus {
      outline: none;
      border-color: #667eea;
      box-shadow: 0 0 0 4px rgba(102, 126, 234, 0.1);
    }

    .filter-input.error {
      border-color: #dc3545;
      background: #fff5f5;
    }

    .error-message {
      color: #dc3545;
      font-size: 13px;
      margin-top: 8px;
      font-weight: 500;
      padding: 12px 16px;
      background: #fff5f5;
      border: 1px solid #fcc;
      border-radius: 8px;
    }

    .search-input {
      width: 100%;
    }

    @media (max-width: 768px) {
      .filters-container {
        padding: 20px;
      }

      .period-buttons {
        flex-direction: column;
        gap: 10px;
      }

      .period-button {
        width: 100%;
      }

      .filter-row {
        flex-direction: column;
      }

      .filter-group,
      .search-group {
        min-width: auto;
        width: 100%;
      }
    }
  `]
})
export class FiltersComponent {
  @Output() filtersChanged = new EventEmitter<EventFilters>();

  filtersForm: FormGroup;
  selectedPeriod: 'today' | 'week' | 'month' | null = null;

  constructor(private fb: FormBuilder) {
    this.filtersForm = this.createForm();
    this.setupFormSubscriptions();
  }

  private createForm(): FormGroup {
    return this.fb.group({
      startDate: [''],
      endDate: [''],
      startTime: [''],
      endTime: [''],
      searchText: ['']
    }, { validators: this.dateRangeValidator });
  }

  private dateRangeValidator = (control: AbstractControl): ValidationErrors | null => {
    const startDate = control.get('startDate')?.value;
    const endDate = control.get('endDate')?.value;
    const startTime = control.get('startTime')?.value;
    const endTime = control.get('endTime')?.value;

    if (!startDate || !endDate) {
      return null;
    }

    const startDateTime = this.combineDateAndTime(startDate, startTime);
    const endDateTime = this.combineDateAndTime(endDate, endTime);

    if (startDateTime && endDateTime && endDateTime < startDateTime) {
      return { invalidDateRange: true };
    }

    return null;
  };

  private combineDateAndTime(date: string, time: string): Date | null {
    if (!date) return null;

    const dateObj = new Date(date);
    if (isNaN(dateObj.getTime())) return null;

    if (time) {
      const [hours, minutes] = time.split(':').map(Number);
      dateObj.setHours(hours, minutes, 0, 0);
    } else {
      dateObj.setHours(0, 0, 0, 0);
    }

    return dateObj;
  }

  private setupFormSubscriptions(): void {
    this.filtersForm.valueChanges.subscribe(() => {
      if (this.filtersForm.valid || !this.hasDateFilters()) {
        this.emitFilters();
      }
    });
  }

  private hasDateFilters(): boolean {
    const formValue = this.filtersForm.value;
    return !!(formValue.startDate || formValue.endDate);
  }

  isDateRangeInvalid(): boolean {
    return this.filtersForm.hasError('invalidDateRange');
  }

  selectPeriod(period: 'today' | 'week' | 'month'): void {
    if (this.selectedPeriod === period) {
      this.selectedPeriod = null;
      this.emitFilters();
      return;
    }

    this.selectedPeriod = period;
    
    this.filtersForm.patchValue({
      startDate: '',
      endDate: '',
      startTime: '',
      endTime: ''
    }, { emitEvent: false });
    
    this.emitFilters();
  }

  private emitFilters(): void {
    if (this.filtersForm.invalid && this.hasDateFilters()) {
      return;
    }

    const formValue = this.filtersForm.value;
    const filters: EventFilters = {};

    if (this.selectedPeriod) {
      filters.periodType = this.selectedPeriod;
    } else {
      if (formValue.startDate) {
        filters.startDate = new Date(formValue.startDate);
      }

      if (formValue.endDate) {
        filters.endDate = new Date(formValue.endDate);
      }

      if (formValue.startTime) {
        filters.startTime = formValue.startTime;
      }

      if (formValue.endTime) {
        filters.endTime = formValue.endTime;
      }
    }

    if (formValue.searchText?.trim()) {
      filters.searchText = formValue.searchText.trim();
    }

    this.filtersChanged.emit(filters);
  }

  clearFilters(): void {
    this.selectedPeriod = null;
    this.filtersForm.reset({
      startDate: '',
      endDate: '',
      startTime: '',
      endTime: '',
      searchText: ''
    });
  }
}
