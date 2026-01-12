import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService, LoginRequest } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-login',
  template: `
    <div class="login-container">
      <div class="login-card">
        <h2 class="login-title">Agenda Manager</h2>
        <p class="login-subtitle">Entre com suas credenciais</p>

        <form [formGroup]="loginForm" (ngSubmit)="onSubmit()" class="login-form">
          <div class="form-group">
            <label for="email" class="form-label">Email</label>
            <input
              type="email"
              id="email"
              formControlName="email"
              class="form-input"
              placeholder="seu@email.com"
              [class.error]="isFieldInvalid('email')"
            >
            <div class="error-message" *ngIf="isFieldInvalid('email')">
              {{ getFieldError('email') }}
            </div>
          </div>

          <div class="form-group">
            <label for="password" class="form-label">Senha</label>
            <input
              type="password"
              id="password"
              formControlName="password"
              class="form-input"
              placeholder="Sua senha"
              [class.error]="isFieldInvalid('password')"
            >
            <div class="error-message" *ngIf="isFieldInvalid('password')">
              {{ getFieldError('password') }}
            </div>
          </div>

          <button
            type="submit"
            class="login-button"
            [disabled]="loginForm.invalid || isLoading"
          >
            <span *ngIf="isLoading">Entrando...</span>
            <span *ngIf="!isLoading">Entrar</span>
          </button>
        </form>

        <div class="error-message" *ngIf="errorMessage">
          {{ errorMessage }}
        </div>

        <div class="register-link">
          <p>Não tem uma conta? <a routerLink="register" class="register-button">Criar conta</a></p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .login-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 100vh;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 50%, #f093fb 100%);
      background-size: 200% 200%;
      animation: gradientShift 8s ease infinite;
      padding: 20px;
      position: relative;
      overflow: hidden;
    }

    @keyframes gradientShift {
      0% { background-position: 0% 50%; }
      50% { background-position: 100% 50%; }
      100% { background-position: 0% 50%; }
    }

    .login-container::before {
      content: '';
      position: absolute;
      top: -50%;
      right: -50%;
      width: 200%;
      height: 200%;
      background: radial-gradient(circle, rgba(255,255,255,0.15) 0%, transparent 70%);
      animation: pulse 20s ease-in-out infinite;
    }

    .login-container::after {
      content: '';
      position: absolute;
      bottom: -30%;
      left: -30%;
      width: 150%;
      height: 150%;
      background: radial-gradient(circle, rgba(240,147,251,0.2) 0%, transparent 70%);
      animation: pulseReverse 25s ease-in-out infinite;
    }

    @keyframes pulse {
      0%, 100% { transform: scale(1) rotate(0deg); opacity: 0.4; }
      50% { transform: scale(1.2) rotate(180deg); opacity: 0.7; }
    }

    @keyframes pulseReverse {
      0%, 100% { transform: scale(1) rotate(0deg); opacity: 0.3; }
      50% { transform: scale(1.15) rotate(-180deg); opacity: 0.6; }
    }

    .login-card {
      background: rgba(255, 255, 255, 0.95);
      backdrop-filter: blur(20px);
      -webkit-backdrop-filter: blur(20px);
      border-radius: 24px;
      padding: 48px;
      box-shadow: 0 25px 80px rgba(0, 0, 0, 0.2), 0 0 0 1px rgba(255, 255, 255, 0.3);
      width: 100%;
      max-width: 420px;
      position: relative;
      z-index: 1;
      animation: slideUp 0.5s ease-out;
      border: 1px solid rgba(255, 255, 255, 0.5);
    }

    @keyframes slideUp {
      from {
        opacity: 0;
        transform: translateY(20px);
      }
      to {
        opacity: 1;
        transform: translateY(0);
      }
    }

    .login-title {
      text-align: center;
      margin-bottom: 8px;
      font-size: 32px;
      font-weight: 700;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
      background-clip: text;
    }

    .login-subtitle {
      text-align: center;
      color: #666;
      margin-bottom: 32px;
      font-size: 16px;
    }

    .login-form {
      display: flex;
      flex-direction: column;
      gap: 20px;
    }

    .form-group {
      display: flex;
      flex-direction: column;
    }

    .form-label {
      margin-bottom: 6px;
      color: #333;
      font-weight: 500;
      font-size: 14px;
    }

    .form-input {
      padding: 14px 18px;
      border: 2px solid #e1e5e9;
      border-radius: 10px;
      font-size: 16px;
      transition: all 0.3s ease;
      background: #f8f9fa;
    }

    .form-input:focus {
      outline: none;
      border-color: #667eea;
      background: white;
      box-shadow: 0 0 0 4px rgba(102, 126, 234, 0.1);
    }

    .form-input.error {
      border-color: #e74c3c;
      background: #fff5f5;
    }

    .error-message {
      color: #e74c3c;
      font-size: 13px;
      margin-top: 6px;
      font-weight: 500;
    }

    .login-button {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
      border: none;
      padding: 16px;
      border-radius: 10px;
      font-size: 16px;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.3s ease;
      margin-top: 8px;
      box-shadow: 0 4px 15px rgba(102, 126, 234, 0.3);
    }

    .login-button:hover:not(:disabled) {
      transform: translateY(-2px);
      box-shadow: 0 8px 25px rgba(102, 126, 234, 0.4);
    }

    .login-button:active:not(:disabled) {
      transform: translateY(0);
    }

    .login-button:disabled {
      opacity: 0.6;
      cursor: not-allowed;
      transform: none;
    }

    .register-link {
      text-align: center;
      margin-top: 24px;
      padding-top: 24px;
      border-top: 1px solid #e1e5e9;
    }

    .register-link p {
      color: #666;
      font-size: 14px;
      margin: 0;
    }

    .register-button {
      color: #667eea;
      text-decoration: none;
      font-weight: 600;
      transition: color 0.3s ease;
    }

    .register-button:hover {
      color: #764ba2;
      text-decoration: underline;
    }

    @media (max-width: 768px) {
      .login-container {
        padding: 16px;
      }

      .login-card {
        padding: 32px 24px;
        border-radius: 20px;
      }

      .login-title {
        font-size: 28px;
      }

      .login-subtitle {
        font-size: 14px;
        margin-bottom: 24px;
      }
    }

    @media (max-width: 480px) {
      .login-card {
        padding: 28px 20px;
        border-radius: 16px;
      }

      .login-title {
        font-size: 24px;
      }

      .form-input {
        padding: 12px 16px;
        font-size: 15px;
      }

      .login-button {
        padding: 14px;
        font-size: 15px;
      }
    }
  `]
})
export class LoginComponent {
  loginForm: FormGroup;
  isLoading = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.loginForm = this.createForm();
  }

  private createForm(): FormGroup {
    return this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  onSubmit(): void {
    if (this.loginForm.valid) {
      this.isLoading = true;
      this.errorMessage = '';

      const credentials: LoginRequest = this.loginForm.value;

      this.authService.login(credentials).subscribe({
        next: () => {
          this.router.navigate(['/dashboard']);
        },
        error: (error) => {
          this.isLoading = false;
          this.errorMessage = error.error?.message || 'Erro ao fazer login. Verifique suas credenciais.';
        }
      });
    } else {
      this.markFormGroupTouched();
    }
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.loginForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  getFieldError(fieldName: string): string {
    const field = this.loginForm.get(fieldName);
    if (field && field.errors) {
      if (field.errors['required']) {
        return `${fieldName === 'email' ? 'Email' : 'Senha'} é obrigatório`;
      }
      if (field.errors['email']) {
        return 'Email inválido';
      }
      if (field.errors['minlength']) {
        return 'Senha deve ter pelo menos 6 caracteres';
      }
    }
    return '';
  }

  private markFormGroupTouched(): void {
    Object.keys(this.loginForm.controls).forEach(field => {
      const control = this.loginForm.get(field);
      control?.markAsTouched();
    });
  }
}
