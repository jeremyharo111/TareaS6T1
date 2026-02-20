import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth/auth';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css'],
  standalone: false
})
export class Dashboard {
  currentUser: string | null;

  constructor(
    private router: Router,
    private authService: AuthService
  ) {
    this.currentUser = this.authService.currentUserValue;
  }

  logout() {
    this.authService.logout().subscribe({
      next: () => {
        this.router.navigate(['/login']);
      },
      error: () => {
        // En caso de error, de igual forma limpia y manda a login
        this.authService.forceLogout();
        this.router.navigate(['/login']);
      }
    });
  }
}
