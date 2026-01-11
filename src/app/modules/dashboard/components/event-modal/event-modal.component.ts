import { Component, EventEmitter, Input, Output, OnInit, OnDestroy, OnChanges, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { EventService, Event, EventType, CreateEventRequest, UpdateEventRequest, UserOption, Participant } from '../../../../core/services/event.service';
import { AuthService } from '../../../../core/auth/auth.service';

@Component({
  selector: 'app-event-modal',
  template: `
    <div class="modal-overlay" (click)="close()" *ngIf="isOpen">
      <div class="modal-container" (click)="$event.stopPropagation()">
        <div class="modal-header">
          <h2 class="modal-title">{{ isEditMode ? 'Editar Evento' : 'Novo Evento' }}</h2>
          <button class="close-button" (click)="close()" type="button">
            <span>×</span>
          </button>
        </div>

        <form [formGroup]="eventForm" (ngSubmit)="onSubmit()" class="modal-form">
          <div class="form-group">
            <label class="form-label">Nome do Evento *</label>
            <input
              type="text"
              formControlName="name"
              class="form-input"
              placeholder="Digite o nome do evento"
              [class.error]="isFieldInvalid('name')"
            >
            <div class="error-message" *ngIf="isFieldInvalid('name')">
              {{ getFieldError('name') }}
            </div>
          </div>

          <div class="form-group">
            <label class="form-label">Descrição *</label>
            <textarea
              formControlName="description"
              class="form-textarea"
              placeholder="Descreva o evento"
              rows="4"
              [class.error]="isFieldInvalid('description')"
            ></textarea>
            <div class="error-message" *ngIf="isFieldInvalid('description')">
              {{ getFieldError('description') }}
            </div>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label class="form-label">Data *</label>
              <input
                type="date"
                formControlName="date"
                class="form-input"
                [class.error]="isFieldInvalid('date')"
              >
              <div class="error-message" *ngIf="isFieldInvalid('date')">
                {{ getFieldError('date') }}
              </div>
            </div>

            <div class="form-group">
              <label class="form-label">Hora *</label>
              <input
                type="time"
                formControlName="time"
                class="form-input"
                [class.error]="isFieldInvalid('time')"
              >
              <div class="error-message" *ngIf="isFieldInvalid('time')">
                {{ getFieldError('time') }}
              </div>
            </div>
          </div>

          <div class="form-group">
            <label class="form-label">Local *</label>
            <input
              type="text"
              formControlName="location"
              class="form-input"
              placeholder="Digite o local do evento"
              [class.error]="isFieldInvalid('location')"
            >
            <div class="error-message" *ngIf="isFieldInvalid('location')">
              {{ getFieldError('location') }}
            </div>
          </div>

          <div class="form-group">
            <label class="form-label">Tipo de Evento *</label>
            <div class="radio-group">
              <label class="radio-option">
                <input
                  type="radio"
                  formControlName="type"
                  [value]="eventType.Exclusive"
                >
                <span class="radio-label">Exclusivo</span>
                <span class="radio-description">Apenas você pode ver</span>
              </label>
              <label class="radio-option">
                <input
                  type="radio"
                  formControlName="type"
                  [value]="eventType.Shared"
                >
                <span class="radio-label">Compartilhado</span>
                <span class="radio-description">Pode ser compartilhado com outros usuários</span>
              </label>
            </div>
          </div>

          <div class="form-group" *ngIf="eventForm.get('type')?.value === eventType.Shared">
            <label class="form-label">Participantes</label>
            <div class="participants-selector">
              <div class="selected-participants" *ngIf="selectedParticipants.length > 0">
                <div class="participant-tag" *ngFor="let participant of selectedParticipants">
                  <span>{{ participant.name }}</span>
                  <button type="button" class="remove-tag" (click)="removeParticipant(participant.id)">
                    ×
                  </button>
                </div>
              </div>
              <div class="search-box">
                <input
                  type="text"
                  class="search-input"
                  placeholder="Buscar usuários..."
                  [(ngModel)]="searchTerm"
                  (input)="filterUsers()"
                  [ngModelOptions]="{standalone: true}"
                >
              </div>
              <div class="users-list" *ngIf="filteredUsers.length > 0">
                <div
                  class="user-option"
                  *ngFor="let user of filteredUsers"
                  (click)="addParticipant(user)"
                >
                  <div class="user-avatar">
                    {{ user.name.charAt(0).toUpperCase() }}
                  </div>
                  <div class="user-info">
                    <div class="user-name">{{ user.name }}</div>
                    <div class="user-email">{{ user.email }}</div>
                  </div>
                  <div class="user-check" *ngIf="isParticipantSelected(user.id)">✓</div>
                </div>
              </div>
              <div class="no-users" *ngIf="filteredUsers.length === 0 && searchTerm">
                Nenhum usuário encontrado
              </div>
            </div>
          </div>

          <div class="error-message" *ngIf="errorMessage">
            {{ errorMessage }}
          </div>

          <div class="modal-actions">
            <button type="button" class="cancel-button" (click)="close()">
              Cancelar
            </button>
            <button type="submit" class="submit-button" [disabled]="eventForm.invalid || isLoading">
              <span *ngIf="isLoading">{{ isEditMode ? 'Salvando...' : 'Criando...' }}</span>
              <span *ngIf="!isLoading">{{ isEditMode ? 'Salvar' : 'Criar Evento' }}</span>
            </button>
          </div>
        </form>
      </div>
    </div>
  `,
  styles: [`
    .modal-overlay {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(0, 0, 0, 0.6);
      backdrop-filter: blur(4px);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 1000;
      padding: 20px;
      animation: fadeIn 0.3s ease;
    }

    @keyframes fadeIn {
      from { opacity: 0; }
      to { opacity: 1; }
    }

    .modal-container {
      background: white;
      border-radius: 20px;
      width: 100%;
      max-width: 600px;
      max-height: 90vh;
      overflow-y: auto;
      box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
      animation: slideUp 0.3s ease;
    }

    @keyframes slideUp {
      from {
        opacity: 0;
        transform: translateY(30px);
      }
      to {
        opacity: 1;
        transform: translateY(0);
      }
    }

    .modal-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 28px 32px;
      border-bottom: 2px solid #f1f3f5;
    }

    .modal-title {
      margin: 0;
      font-size: 24px;
      font-weight: 700;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
      background-clip: text;
    }

    .close-button {
      background: #f8f9fa;
      border: none;
      border-radius: 50%;
      width: 36px;
      height: 36px;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      transition: all 0.2s ease;
      font-size: 24px;
      color: #666;
    }

    .close-button:hover {
      background: #e9ecef;
      transform: rotate(90deg);
    }

    .modal-form {
      padding: 32px;
    }

    .form-group {
      margin-bottom: 24px;
    }

    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 16px;
    }

    .form-label {
      display: block;
      margin-bottom: 8px;
      color: #333;
      font-weight: 600;
      font-size: 14px;
    }

    .form-input,
    .form-textarea {
      width: 100%;
      padding: 14px 18px;
      border: 2px solid #e1e5e9;
      border-radius: 10px;
      font-size: 15px;
      transition: all 0.3s ease;
      background: #f8f9fa;
      font-family: inherit;
    }

    .form-textarea {
      resize: vertical;
      min-height: 100px;
    }

    .form-input:focus,
    .form-textarea:focus {
      outline: none;
      border-color: #667eea;
      background: white;
      box-shadow: 0 0 0 4px rgba(102, 126, 234, 0.1);
    }

    .form-input.error,
    .form-textarea.error {
      border-color: #e74c3c;
      background: #fff5f5;
    }

    .error-message {
      color: #e74c3c;
      font-size: 13px;
      margin-top: 6px;
      font-weight: 500;
    }

    .radio-group {
      display: flex;
      flex-direction: column;
      gap: 12px;
    }

    .radio-option {
      display: flex;
      align-items: center;
      padding: 16px;
      border: 2px solid #e1e5e9;
      border-radius: 12px;
      cursor: pointer;
      transition: all 0.3s ease;
      background: #f8f9fa;
    }

    .radio-option:hover {
      border-color: #667eea;
      background: white;
    }

    .radio-option input[type="radio"] {
      margin-right: 12px;
      width: 20px;
      height: 20px;
      cursor: pointer;
    }

    .radio-option input[type="radio"]:checked + .radio-label {
      color: #667eea;
      font-weight: 600;
    }

    .radio-option:has(input[type="radio"]:checked) {
      border-color: #667eea;
      background: rgba(102, 126, 234, 0.05);
    }

    .radio-label {
      font-weight: 600;
      color: #333;
      margin-right: 8px;
    }

    .radio-description {
      color: #666;
      font-size: 13px;
    }

    .participants-selector {
      border: 2px solid #e1e5e9;
      border-radius: 12px;
      padding: 16px;
      background: #f8f9fa;
      max-height: 300px;
      overflow-y: auto;
    }

    .selected-participants {
      display: flex;
      flex-wrap: wrap;
      gap: 8px;
      margin-bottom: 16px;
    }

    .participant-tag {
      display: flex;
      align-items: center;
      gap: 8px;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
      padding: 8px 14px;
      border-radius: 20px;
      font-size: 14px;
      font-weight: 500;
    }

    .remove-tag {
      background: rgba(255, 255, 255, 0.3);
      border: none;
      border-radius: 50%;
      width: 20px;
      height: 20px;
      cursor: pointer;
      color: white;
      font-size: 16px;
      display: flex;
      align-items: center;
      justify-content: center;
      transition: background 0.2s ease;
    }

    .remove-tag:hover {
      background: rgba(255, 255, 255, 0.5);
    }

    .search-box {
      margin-bottom: 16px;
    }

    .search-input {
      width: 100%;
      padding: 12px 16px;
      border: 2px solid #e1e5e9;
      border-radius: 10px;
      font-size: 14px;
      background: white;
    }

    .search-input:focus {
      outline: none;
      border-color: #667eea;
      box-shadow: 0 0 0 4px rgba(102, 126, 234, 0.1);
    }

    .users-list {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .user-option {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 12px;
      border: 2px solid #e1e5e9;
      border-radius: 10px;
      cursor: pointer;
      transition: all 0.2s ease;
      background: white;
    }

    .user-option:hover {
      border-color: #667eea;
      background: #f8f9fa;
      transform: translateX(4px);
    }

    .user-avatar {
      width: 40px;
      height: 40px;
      border-radius: 50%;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: 600;
      font-size: 16px;
    }

    .user-info {
      flex: 1;
    }

    .user-name {
      font-weight: 600;
      color: #333;
      font-size: 14px;
    }

    .user-email {
      color: #666;
      font-size: 13px;
    }

    .user-check {
      color: #28a745;
      font-size: 20px;
      font-weight: bold;
    }

    .no-users {
      text-align: center;
      padding: 24px;
      color: #666;
      font-size: 14px;
    }

    .modal-actions {
      display: flex;
      justify-content: flex-end;
      gap: 12px;
      margin-top: 32px;
      padding-top: 24px;
      border-top: 2px solid #f1f3f5;
    }

    .cancel-button,
    .submit-button {
      padding: 14px 28px;
      border-radius: 10px;
      font-size: 15px;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.3s ease;
      border: none;
    }

    .cancel-button {
      background: #f8f9fa;
      color: #495057;
      border: 2px solid #e1e5e9;
    }

    .cancel-button:hover {
      background: #e9ecef;
      border-color: #adb5bd;
    }

    .submit-button {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
      box-shadow: 0 4px 15px rgba(102, 126, 234, 0.3);
    }

    .submit-button:hover:not(:disabled) {
      transform: translateY(-2px);
      box-shadow: 0 8px 25px rgba(102, 126, 234, 0.4);
    }

    .submit-button:disabled {
      opacity: 0.6;
      cursor: not-allowed;
      transform: none;
    }

    @media (max-width: 768px) {
      .modal-container {
        max-width: 100%;
        max-height: 95vh;
        border-radius: 16px 16px 0 0;
      }

      .modal-header {
        padding: 20px;
      }

      .modal-form {
        padding: 20px;
      }

      .form-row {
        grid-template-columns: 1fr;
      }

      .modal-actions {
        flex-direction: column-reverse;
      }

      .cancel-button,
      .submit-button {
        width: 100%;
      }
    }
  `]
})
export class EventModalComponent implements OnInit, OnDestroy, OnChanges {
  @Input() isOpen = false;
  @Input() event: Event | null = null;
  @Output() closeModal = new EventEmitter<void>();
  @Output() eventSaved = new EventEmitter<Event>();

  eventForm: FormGroup;
  eventType = EventType;
  isEditMode = false;
  isLoading = false;
  errorMessage = '';
  availableUsers: UserOption[] = [];
  filteredUsers: UserOption[] = [];
  selectedParticipants: UserOption[] = [];
  searchTerm = '';

  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private eventService: EventService,
    private authService: AuthService
  ) {
    this.eventForm = this.createForm();
  }

  ngOnInit(): void {
    this.eventForm.get('type')?.valueChanges.pipe(
      takeUntil(this.destroy$)
    ).subscribe(() => {
      if (this.eventForm.get('type')?.value === EventType.Exclusive) {
        this.selectedParticipants = [];
      }
    });
  }

  

  ngOnChanges(changes: SimpleChanges): void {
    const isOpening = this.isModalOpening(changes);
    const eventChanged = this.isEventChanged(changes);

    if (isOpening || eventChanged) {
      this.initializeModal();
    }
  }

  private isModalOpening(changes: SimpleChanges): boolean {
    return changes['isOpen'] && 
           changes['isOpen'].currentValue && 
           !changes['isOpen'].previousValue;
  }

  private isEventChanged(changes: SimpleChanges): boolean {
    return changes['event'] && 
           changes['event'].currentValue !== changes['event'].previousValue;
  }

  private initializeModal(): void {
    if (!this.isOpen) return;

    this.loadAvailableUsers();
    
    if (this.event) {
      this.setupEditMode();
    } else {
      this.setupCreateMode();
    }
  }

  private setupEditMode(): void {
    this.isEditMode = true;
    this.loadEventData();
  }

  private setupCreateMode(): void {
    this.isEditMode = false;
    this.resetForm();
  }

  private resetForm(): void {
    this.eventForm.reset({
      type: EventType.Exclusive
    });
    this.selectedParticipants = [];
    this.searchTerm = '';
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private createForm(): FormGroup {
    return this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
      description: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(1000)]],
      date: ['', Validators.required],
      time: ['', Validators.required],
      location: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(200)]],
      type: [EventType.Exclusive, Validators.required]
    });
  }

  private loadEventData(): void {
    if (!this.event) return;

    const formData = this.extractFormDataFromEvent();
    this.eventForm.patchValue(formData);
    this.markFormAsValid();
  }

  private markFormAsValid(): void {
    Object.keys(this.eventForm.controls).forEach(key => {
      const control = this.eventForm.get(key);
      if (control) {
        control.markAsTouched();
        control.updateValueAndValidity();
      }
    });
    this.eventForm.updateValueAndValidity();
  }

  private extractFormDataFromEvent(): any {
    const eventDate = new Date(this.event!.date);
    const dateStr = eventDate.toISOString().split('T')[0];
    const timeStr = eventDate.toTimeString().slice(0, 5);

    return {
      name: this.event!.name,
      description: this.event!.description,
      date: dateStr,
      time: timeStr,
      location: this.event!.location,
      type: this.event!.type
    };
  }

  private loadAvailableUsers(): void {
    this.eventService.getActiveUsers().pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: (users) => this.handleUsersLoaded(users),
      error: () => this.handleUsersLoadError()
    });
  }

  private handleUsersLoaded(users: UserOption[]): void {
    this.availableUsers = users;
    this.filteredUsers = users;
    
    if (this.isEditMode && this.event) {
      this.loadEventParticipants(users);
    }
  }

  private loadEventParticipants(users: UserOption[]): void {
    if (!this.event?.participants) return;

    this.selectedParticipants = this.event.participants.map(p => 
      this.mapParticipantToUserOption(p, users)
    );
  }

  private mapParticipantToUserOption(participant: Participant, users: UserOption[]): UserOption {
    const user = users.find(u => u.id === participant.id);
    
    if (user) return user;

    return {
      id: participant.id,
      name: participant.name,
      email: participant.email,
      isActive: true,
      createdAt: new Date()
    };
  }

  private handleUsersLoadError(): void {
    this.errorMessage = 'Erro ao carregar usuários';
  }

  filterUsers(): void {
    if (!this.searchTerm.trim()) {
      this.filteredUsers = this.availableUsers.filter(u => 
        !this.selectedParticipants.some(sp => sp.id === u.id)
      );
      return;
    }

    const search = this.searchTerm.toLowerCase();
    this.filteredUsers = this.availableUsers.filter(u => 
      (u.name.toLowerCase().includes(search) || u.email.toLowerCase().includes(search)) &&
      !this.selectedParticipants.some(sp => sp.id === u.id)
    );
  }

  addParticipant(user: UserOption): void {
    if (!this.isParticipantSelected(user.id)) {
      this.selectedParticipants.push(user);
      this.searchTerm = '';
      this.filterUsers();
    }
  }

  removeParticipant(userId: string): void {
    this.selectedParticipants = this.selectedParticipants.filter(p => p.id !== userId);
    this.filterUsers();
  }

  isParticipantSelected(userId: string): boolean {
    return this.selectedParticipants.some(p => p.id === userId);
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.eventForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  getFieldError(fieldName: string): string {
    const field = this.eventForm.get(fieldName);
    if (field && field.errors) {
      if (field.errors['required']) {
        return 'Este campo é obrigatório';
      }
      if (field.errors['minlength']) {
        return `Mínimo de ${field.errors['minlength'].requiredLength} caracteres`;
      }
      if (field.errors['maxlength']) {
        return `Máximo de ${field.errors['maxlength'].requiredLength} caracteres`;
      }
    }
    return '';
  }

  onSubmit(): void {
    if (!this.eventForm.valid) {
      this.markFormGroupTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const request = this.buildEventRequest();

    if (this.isEditMode && this.event) {
      this.updateEvent(request as UpdateEventRequest);
    } else {
      this.createEvent(request as CreateEventRequest);
    }
  }

  private buildEventRequest(): CreateEventRequest | UpdateEventRequest {
    const formValue = this.eventForm.value;
    const eventDateTime = new Date(`${formValue.date}T${formValue.time}`);

    const baseRequest = {
      name: formValue.name,
      description: formValue.description,
      date: eventDateTime,
      location: formValue.location,
      type: formValue.type,
      participantIds: this.selectedParticipants.map(p => p.id)
    };

    if (this.isEditMode && this.event) {
      return { ...baseRequest, id: this.event.id } as UpdateEventRequest;
    }

    return baseRequest as CreateEventRequest;
  }

  private createEvent(request: CreateEventRequest): void {
    this.eventService.createEvent(request).pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: (event) => this.handleEventSaved(event),
      error: (error) => this.handleError(error, 'Erro ao criar evento')
    });
  }

  private updateEvent(request: UpdateEventRequest): void {
    this.eventService.updateEvent(request).pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: (event) => this.handleEventSaved(event),
      error: (error) => this.handleError(error, 'Erro ao atualizar evento')
    });
  }

  private handleEventSaved(event: Event): void {
    this.eventSaved.emit(event);
    this.close();
  }

  private handleError(error: any, defaultMessage: string): void {
    this.isLoading = false;
    this.errorMessage = error.error?.message || defaultMessage;
  }

  private markFormGroupTouched(): void {
    Object.keys(this.eventForm.controls).forEach(field => {
      const control = this.eventForm.get(field);
      control?.markAsTouched();
    });
  }

  close(): void {
    this.isOpen = false;
    this.eventForm.reset();
    this.selectedParticipants = [];
    this.searchTerm = '';
    this.errorMessage = '';
    this.isEditMode = false;
    this.event = null;
    this.closeModal.emit();
  }
}
