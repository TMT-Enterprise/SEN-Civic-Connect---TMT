import { Component, input } from '@angular/core';

@Component({
  selector: 'app-metric-card',
  standalone: true,
  imports: [],
  templateUrl: './metric-card.html',
  styleUrl: './metric-card.css',
})
export class MetricCard {
  label = input('');
  value = input('0');
  detail = input('');
  icon = input('');
  tone = input<'secondary' | 'info' | 'warning' | 'primary' | 'purple'>('secondary');
}