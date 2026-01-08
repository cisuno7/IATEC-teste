import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subject, takeUntil } from 'rxjs';
import { AuthService, User } from '../../../core/auth/auth.service';
import { EventService, Event, EventFilters } from '../../../core/services/event.service';
import { ParticipantsComponent } from '../components/participants/participants.component';

@Component({
  selector: 'app-dashboard-page',
  template: `
    <div class="dashboard-container">
      <header class="dashboard-header">
        <div class="header-content">
          <h1 class="dashboard-title">Agenda Manager</h1>
          <div class="user-info">
            <span class="welcome-text">Olá, {{ getWelcomeName() }}</span>
            <button class="logout-button" (click)="logout()">
              Sair
            </button>
          </div>
        </div>
      </header>

      <main class="dashboard-main">
        <div class="actions-bar">
          <button class="create-button" (click)="openCreateEventModal()">
            <span>+</span> Novo Evento
          </button>
        </div>

        <app-filters (filtersChanged)="onFiltersChanged($event)"></app-filters>

        <div class="events-section">
          <div class="section-header">
            <h2>Seus Eventos</h2>
            <div class="events-count">{{ events.length }} evento(s)</div>
          </div>

          <div class="loading" *ngIf="isLoading">
            Carregando eventos...
          </div>

          <div class="no-events" *ngIf="!isLoading && events.length === 0">
            <p>Nenhum evento encontrado.</p>
            <p *ngIf="hasActiveFilters">Tente ajustar os filtros ou <button class="link-button" (click)="clearFilters()">limpe-os</button>.</p>
          </div>

          <div class="events-grid" *ngIf="!isLoading && events.length > 0">
            <div class="event-card" *ngFor="let event of events" [class.shared]="event.type === 1">
              <div class="event-header">
                <h3 class="event-title">{{ event.name }}</h3>
                <div class="event-actions">
                  <button class="action-button edit" (click)="editEvent(event)" title="Editar">
                    ✏️
                  </button>
                  <button class="action-button delete" (click)="deleteEvent(event)" title="Excluir">
                    🗑️
                  </button>
                </div>
              </div>

              <div class="event-details">
                <p class="event-description" *ngIf="event.description">
                  {{ event.description }}
                </p>

                <div class="event-meta">
                  <div class="meta-item">
                    <span class="meta-icon">📅</span>
                    <span>{{ formatDate(event.date) }}</span>
                  </div>

                  <div class="meta-item" *ngIf="event.location">
                    <span class="meta-icon">📍</span>
                    <span>{{ event.location }}</span>
                  </div>

                  <div class="meta-item">
                    <span class="meta-icon">👤</span>
                    <span>{{ event.creatorName }}</span>
                  </div>

                  <div class="meta-item" *ngIf="event.type === 1">
                    <span class="meta-icon">👥</span>
                    <span>{{ event.participants.length }} participante(s)</span>
                  </div>
                </div>
              </div>

              <div class="event-actions-bottom">
                <button
                  class="action-button toggle-status"
                  [class.activate]="!event.isActive"
                  [class.deactivate]="event.isActive"
                  (click)="toggleEventStatus(event)"
                  title="{{ event.isActive ? 'Desativar' : 'Ativar' }} evento">
                  {{ event.isActive ? 'Desativar' : 'Ativar' }}
                </button>

                <div class="event-status" [class.inactive]="!event.isActive">
                  {{ event.isActive ? 'Ativo' : 'Inativo' }}
                </div>
              </div>

              <div class="event-participants" *ngIf="event.participants && event.participants.length > 0">
                <app-participants
                  [participants]="event.participants"
                  [canManageParticipants]="canManageEvent(event)"
                  (removeParticipant)="removeParticipant(event.id, $event)">
                </app-participants>
              </div>
            </div>
          </div>
        </div>
      </main>
    </div>
  `,
  styles: [`
    .dashboard-container {
      min-height: 100vh;
      background: linear-gradient(135deg, #f5f7fa 0%, #e9ecef 100%);
    }

    .dashboard-header {
      background: linear-gradient(135deg, #ffffff 0%, #f8f9fa 100%);
      border-bottom: 1px solid #e1e5e9;
      padding: 0 32px;
      box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
      position: sticky;
      top: 0;
      z-index: 100;
    }

    .header-content {
      max-width: 1400px;
      margin: 0 auto;
      display: flex;
      justify-content: space-between;
      align-items: center;
      height: 80px;
    }

    .dashboard-title {
      margin: 0;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
      background-clip: text;
      font-size: 28px;
      font-weight: 700;
      letter-spacing: -0.02em;
    }

    .user-info {
      display: flex;
      align-items: center;
      gap: 20px;
    }

    .welcome-text {
      color: #495057;
      font-size: 15px;
      font-weight: 500;
    }

    .logout-button {
      background: linear-gradient(135deg, #dc3545 0%, #c82333 100%);
      color: white;
      border: none;
      padding: 10px 20px;
      border-radius: 10px;
      cursor: pointer;
      font-size: 14px;
      font-weight: 600;
      transition: all 0.3s ease;
      box-shadow: 0 2px 8px rgba(220, 53, 69, 0.2);
    }

    .logout-button:hover {
      transform: translateY(-2px);
      box-shadow: 0 4px 12px rgba(220, 53, 69, 0.3);
    }

    .dashboard-main {
      max-width: 1400px;
      margin: 0 auto;
      padding: 32px;
    }

    .actions-bar {
      margin-bottom: 32px;
    }

    .create-button {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
      border: none;
      padding: 16px 32px;
      border-radius: 12px;
      cursor: pointer;
      font-size: 16px;
      font-weight: 700;
      display: inline-flex;
      align-items: center;
      gap: 10px;
      transition: all 0.3s ease;
      box-shadow: 0 4px 16px rgba(102, 126, 234, 0.3);
    }

    .create-button:hover {
      transform: translateY(-3px);
      box-shadow: 0 8px 24px rgba(102, 126, 234, 0.4);
    }

    .create-button span {
      font-size: 24px;
      font-weight: 300;
      line-height: 1;
    }

    .events-section {
      background: white;
      border-radius: 20px;
      padding: 32px;
      box-shadow: 0 8px 32px rgba(0, 0, 0, 0.08);
      border: 1px solid #e9ecef;
    }

    .section-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 32px;
      padding-bottom: 20px;
      border-bottom: 2px solid #f1f3f5;
    }

    .section-header h2 {
      margin: 0;
      color: #1a1a1a;
      font-size: 24px;
      font-weight: 700;
      letter-spacing: -0.02em;
    }

    .events-count {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
      padding: 8px 16px;
      border-radius: 20px;
      font-size: 14px;
      font-weight: 600;
    }

    .loading, .no-events {
      text-align: center;
      padding: 64px 32px;
      color: #6c757d;
    }

    .loading {
      font-size: 16px;
      font-weight: 500;
    }

    .no-events p {
      margin: 12px 0;
      font-size: 15px;
    }

    .link-button {
      background: none;
      border: none;
      color: #667eea;
      cursor: pointer;
      text-decoration: underline;
      font-size: inherit;
      font-weight: 600;
      transition: color 0.2s ease;
    }

    .link-button:hover {
      color: #764ba2;
    }

    .events-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(380px, 1fr));
      gap: 24px;
    }

    .event-card {
      background: linear-gradient(135deg, #ffffff 0%, #f8f9fa 100%);
      border: 2px solid #e1e5e9;
      border-radius: 16px;
      padding: 24px;
      transition: all 0.3s ease;
      position: relative;
      overflow: hidden;
    }

    .event-card::before {
      content: '';
      position: absolute;
      top: 0;
      left: 0;
      width: 4px;
      height: 100%;
      background: linear-gradient(180deg, #667eea 0%, #764ba2 100%);
      opacity: 0;
      transition: opacity 0.3s ease;
    }

    .event-card:hover {
      border-color: #667eea;
      box-shadow: 0 8px 24px rgba(102, 126, 234, 0.15);
      transform: translateY(-4px);
    }

    .event-card:hover::before {
      opacity: 1;
    }

    .event-card.shared {
      border-left: 4px solid #28a745;
    }

    .event-card.shared::before {
      background: linear-gradient(180deg, #28a745 0%, #20c997 100%);
      opacity: 1;
      width: 4px;
    }

    .event-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 16px;
    }

    .event-title {
      margin: 0;
      color: #1a1a1a;
      font-size: 20px;
      font-weight: 700;
      flex: 1;
      margin-right: 12px;
      line-height: 1.3;
      letter-spacing: -0.01em;
    }

    .event-actions {
      display: flex;
      gap: 6px;
    }

    .action-button {
      background: #f8f9fa;
      border: 2px solid #e9ecef;
      cursor: pointer;
      padding: 8px;
      border-radius: 8px;
      transition: all 0.2s ease;
      font-size: 16px;
      width: 36px;
      height: 36px;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .action-button.edit:hover {
      background: #e3f2fd;
      border-color: #2196f3;
      transform: scale(1.1);
    }

    .action-button.delete:hover {
      background: #ffeaea;
      border-color: #dc3545;
      transform: scale(1.1);
    }

    .event-details {
      margin-bottom: 20px;
    }

    .event-description {
      color: #6c757d;
      margin-bottom: 16px;
      line-height: 1.6;
      font-size: 14px;
    }

    .event-meta {
      display: flex;
      flex-direction: column;
      gap: 10px;
      padding: 16px;
      background: #f8f9fa;
      border-radius: 12px;
    }

    .meta-item {
      display: flex;
      align-items: center;
      gap: 10px;
      font-size: 14px;
      color: #495057;
      font-weight: 500;
    }

    .meta-icon {
      font-size: 16px;
      width: 20px;
      text-align: center;
    }

    .event-actions-bottom {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-top: 16px;
      padding-top: 16px;
      border-top: 1px solid #e9ecef;
    }

    .toggle-status {
      padding: 10px 20px;
      border-radius: 8px;
      border: 2px solid;
      cursor: pointer;
      font-size: 13px;
      font-weight: 600;
      transition: all 0.2s ease;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }

    .toggle-status.activate {
      background: #28a745;
      border-color: #28a745;
      color: white;
    }

    .toggle-status.activate:hover {
      background: #218838;
      border-color: #218838;
    }

    .toggle-status.deactivate {
      background: #ffc107;
      border-color: #ffc107;
      color: #212529;
    }

    .toggle-status.deactivate:hover {
      background: #e0a800;
      border-color: #e0a800;
    }

    .event-status {
      padding: 6px 14px;
      border-radius: 20px;
      font-size: 11px;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      color: white;
      background: linear-gradient(135deg, #28a745 0%, #20c997 100%);
    }

    .event-status.inactive {
      background: linear-gradient(135deg, #dc3545 0%, #c82333 100%);
    }

    @media (max-width: 768px) {
      .dashboard-main {
        padding: 20px;
      }

      .header-content {
        flex-direction: column;
        height: auto;
        padding: 20px 0;
        gap: 16px;
      }

      .dashboard-title {
        font-size: 24px;
      }

      .events-grid {
        grid-template-columns: 1fr;
        gap: 16px;
      }

      .events-section {
        padding: 20px;
      }

      .event-header {
        flex-direction: column;
        gap: 12px;
      }

      .event-actions {
        align-self: flex-end;
      }
    }
  `]
})
export class DashboardPageComponent implements OnInit, OnDestroy {
  currentUser: User | null = null;
  events: Event[] = [];
  isLoading = false;
  hasActiveFilters = false;
  currentFilters: EventFilters = {};

  private destroy$ = new Subject<void>();

  constructor(
    private authService: AuthService,
    private eventService: EventService
  ) { }

  ngOnInit(): void {
    this.authService.currentUser$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(user => {
      this.currentUser = user;
    });

    this.loadEvents();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadEvents(): void {
    this.isLoading = true;
    this.eventService.getDashboardEvents(this.currentFilters).pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: (events) => {
        this.events = events;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Erro ao carregar eventos:', error);
        this.isLoading = false;
      }
    });
  }

  onFiltersChanged(filters: EventFilters): void {
    this.currentFilters = filters;
    this.hasActiveFilters = Object.keys(filters).some(key =>
      filters[key as keyof EventFilters] !== undefined &&
      filters[key as keyof EventFilters] !== null &&
      filters[key as keyof EventFilters] !== ''
    );
    this.loadEvents();
  }

  clearFilters(): void {
    this.currentFilters = {};
    this.hasActiveFilters = false;
    this.loadEvents();
  }

  formatDate(date: Date | string): string {
    const d = new Date(date);
    return d.toLocaleDateString('pt-BR', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  logout(): void {
    this.authService.logout().subscribe();
  }

  openCreateEventModal(): void {
    console.log('Abrir modal de criação de evento');
  }

  editEvent(event: Event): void {
    console.log('Editar evento:', event);
  }

  deleteEvent(event: Event): void {
    if (confirm(`Tem certeza que deseja excluir o evento "${event.name}"?`)) {
      this.eventService.deleteEvent(event.id).pipe(
        takeUntil(this.destroy$)
      ).subscribe({
        next: () => {
          this.loadEvents();
        },
        error: (error) => {
          console.error('Erro ao excluir evento:', error);
        }
      });
    }
  }

  toggleEventStatus(event: Event): void {
    const action = event.isActive ? 'desativar' : 'ativar';
    if (confirm(`Tem certeza que deseja ${action} o evento "${event.name}"?`)) {
      const operation = event.isActive
        ? this.eventService.deactivateEvent(event.id)
        : this.eventService.activateEvent(event.id);

      operation.pipe(takeUntil(this.destroy$)).subscribe({
        next: () => {
          this.loadEvents();
        },
        error: (error) => {
          console.error(`Erro ao ${action} evento:`, error);
        }
      });
    }
  }

  removeParticipant(eventId: string, userId: string): void {
    if (confirm('Tem certeza que deseja remover este participante?')) {
      this.eventService.removeParticipant(eventId, userId).pipe(
        takeUntil(this.destroy$)
      ).subscribe({
        next: () => {
          this.loadEvents();
        },
        error: (error) => {
          console.error('Erro ao remover participante:', error);
        }
      });
    }
  }

  canManageEvent(event: Event): boolean {
    return event.creatorId === this.currentUser?.id;
  }

  getWelcomeName(): string {
    return this.currentUser?.name || 'Usuário';
  }
}
