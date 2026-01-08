import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService, RegisterRequest } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-register',
  template: `
    <div class="register-container">
      <div class="register-card">
        <div class="register-header">
          <h2 class="register-title">Criar Conta</h2>
          <p class="register-subtitle">Preencha os dados para começar</p>
        </div>

        <form [formGroup]="registerForm" (ngSubmit)="onSubmit()" class="register-form">
          <div class="form-group">
            <label for="name" class="form-label">Nome Completo</label>
            <input
              type="text"
              id="name"
              formControlName="name"
              class="form-input"
              placeholder="Seu nome completo"
              [class.error]="isFieldInvalid('name')"
            >
            <div class="error-message" *ngIf="isFieldInvalid('name')">
              {{ getFieldError('name') }}
            </div>
          </div>

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
              placeholder="Mínimo 6 caracteres"
              [class.error]="isFieldInvalid('password')"
            >
            <div class="error-message" *ngIf="isFieldInvalid('password')">
              {{ getFieldError('password') }}
            </div>
          </div>

          <div class="form-group">
            <label for="confirmPassword" class="form-label">Confirmar Senha</label>
            <input
              type="password"
              id="confirmPassword"
              formControlName="confirmPassword"
              class="form-input"
              placeholder="Digite a senha novamente"
              [class.error]="isFieldInvalid('confirmPassword')"
            >
            <div class="error-message" *ngIf="isFieldInvalid('confirmPassword')">
              {{ getFieldError('confirmPassword') }}
            </div>
          </div>

          <button
            type="submit"
            class="register-button"
            [disabled]="registerForm.invalid || isLoading"
          >
            <span *ngIf="isLoading">Criando conta...</span>
            <span *ngIf="!isLoading">Criar Conta</span>
          </button>
        </form>

        <div class="error-message" *ngIf="errorMessage">
          {{ errorMessage }}
        </div>

        <div class="login-link">
          <p>Já tem uma conta? <a routerLink="" class="login-button-link">Fazer login</a></p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .register-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 100vh;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      padding: 20px;
    }

    .register-card {
      background: white;
      border-radius: 16px;
      padding: 48px;
      box-shadow: 0 20px 60px rgba(0, 0, 0, 0.15);
      width: 100%;
      max-width: 450px;
      animation: slideUp 0.4s ease-out;
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

    .register-header {
      text-align: center;
      margin-bottom: 32px;
    }

    .register-title {
      color: #333;
      margin-bottom: 8px;
      font-size: 32px;
      font-weight: 700;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
      background-clip: text;
    }

    .register-subtitle {
      color: #666;
      font-size: 16px;
      margin: 0;
    }

    .register-form {
      display: flex;
      flex-direction: column;
      gap: 20px;
    }

    .form-group {
      display: flex;
      flex-direction: column;
    }

    .form-label {
      margin-bottom: 8px;
      color: #333;
      font-weight: 600;
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

    .register-button {
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

    .register-button:hover:not(:disabled) {
      transform: translateY(-2px);
      box-shadow: 0 8px 25px rgba(102, 126, 234, 0.4);
    }

    .register-button:active:not(:disabled) {
      transform: translateY(0);
    }

    .register-button:disabled {
      opacity: 0.6;
      cursor: not-allowed;
      transform: none;
    }

    .login-link {
      text-align: center;
      margin-top: 24px;
      padding-top: 24px;
      border-top: 1px solid #e1e5e9;
    }

    .login-link p {
      color: #666;
      font-size: 14px;
      margin: 0;
    }

    .login-button-link {
      color: #667eea;
      text-decoration: none;
      font-weight: 600;
      transition: color 0.3s ease;
    }

    .login-button-link:hover {
      color: #764ba2;
      text-decoration: underline;
    }
  `]
})
export class RegisterComponent {
  registerForm: FormGroup;
  isLoading = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.registerForm = this.createForm();
  }

  private createForm(): FormGroup {
    return this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  private passwordMatchValidator(form: FormGroup) {
    const password = form.get('password');
    const confirmPassword = form.get('confirmPassword');
    
    if (password && confirmPassword && password.value !== confirmPassword.value) {
      confirmPassword.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    }
    
    return null;
  }

  onSubmit(): void {
    if (this.registerForm.valid) {
      this.isLoading = true;
      this.errorMessage = '';

      const registerData: RegisterRequest = {
        name: this.registerForm.value.name,
        email: this.registerForm.value.email,
        password: this.registerForm.value.password
      };

      this.authService.register(registerData).subscribe({
        next: () => {
          this.router.navigate(['/login']);
        },
        error: (error) => {
          this.isLoading = false;
          this.errorMessage = error.error?.message || 'Erro ao criar conta. Tente novamente.';
        }
      });
    } else {
      this.markFormGroupTouched();
    }
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.registerForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  getFieldError(fieldName: string): string {
    const field = this.registerForm.get(fieldName);
    if (field && field.errors) {
      if (field.errors['required']) {
        if (fieldName === 'name') return 'Nome é obrigatório';
        if (fieldName === 'email') return 'Email é obrigatório';
        if (fieldName === 'password') return 'Senha é obrigatória';
        if (fieldName === 'confirmPassword') return 'Confirmação de senha é obrigatória';
      }
      if (field.errors['email']) {
        return 'Email inválido';
      }
      if (field.errors['minlength']) {
        if (fieldName === 'name') return 'Nome deve ter pelo menos 2 caracteres';
        if (fieldName === 'password') return 'Senha deve ter pelo menos 6 caracteres';
      }
      if (field.errors['maxlength']) {
        return 'Nome não pode ter mais de 100 caracteres';
      }
      if (field.errors['passwordMismatch']) {
        return 'As senhas não coincidem';
      }
    }
    return '';
  }

  private markFormGroupTouched(): void {
    Object.keys(this.registerForm.controls).forEach(field => {
      const control = this.registerForm.get(field);
      control?.markAsTouched();
    });
  }
}
