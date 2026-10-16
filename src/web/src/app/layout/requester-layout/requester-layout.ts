import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AppHeader } from '../shared/app-header/app-header';
import { BottomNav } from '../../shared/bottom-nav/bottom-nav';

@Component({
  selector: 'app-requester-layout',
  standalone: true,
  imports: [
    RouterOutlet,
    AppHeader,
    BottomNav
  ],
  templateUrl: './requester-layout.html',
  styleUrl: './requester-layout.css',
})
export class RequesterLayout {}
