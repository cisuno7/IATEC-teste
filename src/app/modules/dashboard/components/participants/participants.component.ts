import { Component, Input, Output, EventEmitter } from '@angular/core';
import { Participant } from '../../../../core/services/event.service';

@Component({
  selector: 'app-participants',
  template: `
    <div class="participants-container" *ngIf="participants && participants.length > 0">
      <h4>Participantes ({{ participants.length }})</h4>
      <div class="participants-list">
        <div class="participant-item" *ngFor="let participant of participants">
          <div class="participant-info">
            <div class="participant-avatar">
              {{ participant.name.charAt(0).toUpperCase() }}
            </div>
            <div class="participant-details">
              <div class="participant-name">{{ participant.name }}</div>
              <div class="participant-email">{{ participant.email }}</div>
            </div>
          </div>
          <button
            class="remove-button"
            (click)="onRemoveParticipant(participant.id)"
            *ngIf="canManageParticipants"
            title="Remover participante">
            ×
          </button>
        </div>
      </div>
    </div>

    <div class="no-participants" *ngIf="!participants || participants.length === 0">
      <p>Nenhum participante</p>
    </div>
  `,
  styles: [`
    .participants-container {
      margin-top: 16px;
    }

    .participants-container h4 {
      margin: 0 0 12px 0;
      color: #555;
      font-size: 14px;
      font-weight: 600;
    }

    .participants-list {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .participant-item {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 8px 12px;
      background: #f8f9fa;
      border-radius: 8px;
      border: 1px solid #e9ecef;
    }

    .participant-info {
      display: flex;
      align-items: center;
      gap: 12px;
    }

    .participant-avatar {
      width: 32px;
      height: 32px;
      border-radius: 50%;
      background: #007bff;
      color: white;
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: 600;
      font-size: 14px;
    }

    .participant-details {
      display: flex;
      flex-direction: column;
    }

    .participant-name {
      font-weight: 500;
      font-size: 14px;
      color: #333;
      margin: 0;
    }

    .participant-email {
      font-size: 12px;
      color: #666;
      margin: 0;
    }

    .remove-button {
      background: #dc3545;
      color: white;
      border: none;
      border-radius: 50%;
      width: 24px;
      height: 24px;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 16px;
      transition: background-color 0.2s ease;
    }

    .remove-button:hover {
      background: #c82333;
    }

    .no-participants {
      text-align: center;
      padding: 24px;
      color: #666;
    }

    .no-participants p {
      margin: 0;
      font-size: 14px;
    }
  `]
})
export class ParticipantsComponent {
  @Input() participants: Participant[] = [];
  @Input() canManageParticipants = false;
  @Output() removeParticipant = new EventEmitter<string>();

  onRemoveParticipant(participantId: string) {
    this.removeParticipant.emit(participantId);
  }
}
