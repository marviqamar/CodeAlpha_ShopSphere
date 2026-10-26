import { Component, inject, signal } from '@angular/core';
import { Router, RouterOutlet, NavigationEnd } from '@angular/router';
import { Navbar } from './layout/navbar/navbar';
import { Footer } from './layout/footer/footer';

const HIDDEN_CHROME_PREFIXES = ['/admin', '/login', '/register'];

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Navbar, Footer],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  private router = inject(Router);
  showStoreChrome = signal(true);

  constructor() {
    this.router.events.subscribe(event => {
      if (event instanceof NavigationEnd) {
        const hide = HIDDEN_CHROME_PREFIXES.some(prefix =>
          event.urlAfterRedirects.startsWith(prefix)
        );
        this.showStoreChrome.set(!hide);
      }
    });
  }
}