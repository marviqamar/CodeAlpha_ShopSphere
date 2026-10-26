import { Component, signal } from '@angular/core';

@Component({
  selector: 'app-confirm-dialog',
  imports: [],
  templateUrl: './confirm-dialog.html',
  styleUrl: './confirm-dialog.css'
})
export class ConfirmDialog {
  isOpen = signal(false);
  title = signal('Are you sure?');
  message = signal('');
  confirmLabel = signal('Delete');

  private onConfirmCallback: (() => void) | null = null;

  open(config: { title?: string; message: string; confirmLabel?: string; onConfirm: () => void }): void {
    this.title.set(config.title ?? 'Are you sure?');
    this.message.set(config.message);
    this.confirmLabel.set(config.confirmLabel ?? 'Delete');
    this.onConfirmCallback = config.onConfirm;
    this.isOpen.set(true);
  }

  confirm(): void {
    this.onConfirmCallback?.();
    this.close();
  }

  close(): void {
    this.isOpen.set(false);
    this.onConfirmCallback = null;
  }
}