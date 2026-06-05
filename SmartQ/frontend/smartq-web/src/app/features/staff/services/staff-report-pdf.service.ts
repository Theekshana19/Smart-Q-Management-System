import { Injectable } from '@angular/core';
import { jsPDF } from 'jspdf';
import autoTable from 'jspdf-autotable';
import { StaffPerformance, StaffTokenHistoryItem } from '../../../core/models/staff-console.models';

export interface ReportContext {
  branchName: string;
  counterLabel: string;
  staffName?: string;
  periodLabel: string;
  filtersLabel: string;
}

interface PdfTheme {
  primary: [number, number, number];
  secondary: [number, number, number];
  surface: [number, number, number];
  text: [number, number, number];
  muted: [number, number, number];
  border: [number, number, number];
  white: [number, number, number];
}

@Injectable({ providedIn: 'root' })
export class StaffReportPdfService {
  private readonly theme: PdfTheme = {
    primary: [0, 0, 0],
    secondary: [0, 106, 102],
    surface: [248, 249, 255],
    text: [11, 28, 48],
    muted: [69, 70, 77],
    border: [211, 228, 254],
    white: [255, 255, 255]
  };

  downloadJson(filename: string, payload: unknown): void {
    const blob = new Blob([JSON.stringify(payload, null, 2)], { type: 'application/json' });
    this.triggerDownload(blob, filename);
  }

  viewTokenHistoryPdf(
    items: StaffTokenHistoryItem[],
    context: ReportContext,
    summary: { served: number; completed: number; skipped: number; avgServiceTime: string }
  ): void {
    this.openPdf(this.buildTokenHistoryPdf(items, context, summary));
  }

  exportTokenHistoryPdf(
    items: StaffTokenHistoryItem[],
    context: ReportContext,
    summary: { served: number; completed: number; skipped: number; avgServiceTime: string }
  ): void {
    const doc = this.buildTokenHistoryPdf(items, context, summary);
    doc.save(this.fileName('token-history', context.periodLabel));
  }

  viewPerformancePdf(performance: StaffPerformance, context: ReportContext): void {
    this.openPdf(this.buildPerformancePdf(performance, context));
  }

  exportPerformancePdf(performance: StaffPerformance, context: ReportContext): void {
    const doc = this.buildPerformancePdf(performance, context);
    doc.save(this.fileName('performance-analytics', context.periodLabel));
  }

  private buildTokenHistoryPdf(
    items: StaffTokenHistoryItem[],
    context: ReportContext,
    summary: { served: number; completed: number; skipped: number; avgServiceTime: string }
  ): jsPDF {
    const doc = new jsPDF({ orientation: 'portrait', unit: 'mm', format: 'a4' });
    let y = this.drawReportHeader(doc, 'Token History Report', context);

    y = this.drawSummaryStrip(doc, y, [
      { label: 'Total Records', value: `${items.length}` },
      { label: 'Completed', value: `${summary.completed}` },
      { label: 'Skipped', value: `${summary.skipped}` },
      { label: 'Avg Service', value: summary.avgServiceTime }
    ]);

    autoTable(doc, {
      startY: y + 4,
      head: [['Token No', 'Service Type', 'Called Time', 'Duration', 'Status']],
      body: items.map((row) => [
        row.tokenNo,
        row.serviceType,
        this.formatTime(row.calledTime),
        row.duration,
        row.status
      ]),
      styles: {
        font: 'helvetica',
        fontSize: 9,
        cellPadding: 3,
        textColor: this.theme.text,
        lineColor: this.theme.border,
        lineWidth: 0.1
      },
      headStyles: {
        fillColor: this.theme.secondary,
        textColor: this.theme.white,
        fontStyle: 'bold',
        halign: 'left'
      },
      alternateRowStyles: { fillColor: this.theme.surface },
      columnStyles: {
        0: { fontStyle: 'bold', cellWidth: 28 },
        2: { cellWidth: 28 },
        3: { cellWidth: 24 },
        4: { cellWidth: 28 }
      },
      margin: { left: 14, right: 14 },
      didDrawPage: (data) => this.drawPageFooter(doc, data.pageNumber)
    });

    return doc;
  }

  private buildPerformancePdf(performance: StaffPerformance, context: ReportContext): jsPDF {
    const doc = new jsPDF({ orientation: 'portrait', unit: 'mm', format: 'a4' });
    let y = this.drawReportHeader(doc, 'Performance Analytics Report', context);

    y = this.drawSummaryStrip(doc, y, [
      { label: performance.servedLabel, value: `${performance.servedToday}`, trend: performance.servedTrendLabel },
      { label: 'Avg Service Time', value: performance.avgServiceTime, trend: performance.avgServiceTimeTrendLabel },
      { label: 'Completion Rate', value: `${performance.completionRate}%`, trend: performance.completionTrendLabel }
    ]);

    y = this.drawSectionTitle(doc, y, 'Hourly Traffic Distribution');
    autoTable(doc, {
      startY: y + 2,
      head: [['Hour', 'Cash', 'Account', 'Loan', 'Total']],
      body: performance.hourlyTraffic.map((h) => {
        const total = h.cashCount + h.accountCount + h.loanCount;
        return [h.hourLabel, `${h.cashCount}`, `${h.accountCount}`, `${h.loanCount}`, `${total}`];
      }),
      styles: {
        fontSize: 9,
        cellPadding: 3,
        textColor: this.theme.text,
        lineColor: this.theme.border,
        lineWidth: 0.1
      },
      headStyles: {
        fillColor: this.theme.primary,
        textColor: this.theme.white,
        fontStyle: 'bold'
      },
      alternateRowStyles: { fillColor: this.theme.surface },
      margin: { left: 14, right: 14 }
    });

    y = (doc as jsPDF & { lastAutoTable: { finalY: number } }).lastAutoTable.finalY + 10;
    y = this.drawSectionTitle(doc, y, 'Live Timeline');

    autoTable(doc, {
      startY: y + 2,
      head: [['Event', 'Description', 'Metric', 'Time']],
      body: performance.recentTimeline.map((item) => [
        item.title,
        item.description,
        item.metricLabel && item.metricValue ? `${item.metricLabel}: ${item.metricValue}` : '—',
        this.formatDateTime(item.timestamp)
      ]),
      styles: {
        fontSize: 8.5,
        cellPadding: 2.5,
        textColor: this.theme.text,
        lineColor: this.theme.border,
        lineWidth: 0.1
      },
      headStyles: {
        fillColor: this.theme.secondary,
        textColor: this.theme.white,
        fontStyle: 'bold'
      },
      alternateRowStyles: { fillColor: this.theme.surface },
      margin: { left: 14, right: 14 }
    });

    y = (doc as jsPDF & { lastAutoTable: { finalY: number } }).lastAutoTable.finalY + 10;
    y = this.drawTipBox(doc, y, performance.optimizationTip);

    this.drawPageFooter(doc, doc.getNumberOfPages());
    return doc;
  }

  private openPdf(doc: jsPDF): void {
    const blob = doc.output('blob');
    const url = URL.createObjectURL(blob);
    window.open(url, '_blank', 'noopener,noreferrer');
    setTimeout(() => URL.revokeObjectURL(url), 120_000);
  }

  private drawReportHeader(doc: jsPDF, title: string, context: ReportContext): number {
    const pageWidth = doc.internal.pageSize.getWidth();
    doc.setFillColor(...this.theme.primary);
    doc.rect(0, 0, pageWidth, 32, 'F');

    doc.setTextColor(...this.theme.white);
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(16);
    doc.text('Global Finance', 14, 12);

    doc.setFont('helvetica', 'normal');
    doc.setFontSize(9);
    doc.text('Staff Console — Queue Management', 14, 18);

    doc.setFont('helvetica', 'bold');
    doc.setFontSize(13);
    doc.text(title, 14, 26);

    doc.setFont('helvetica', 'normal');
    doc.setFontSize(8);
    doc.text(`Generated: ${this.formatDateTime(new Date().toISOString())}`, pageWidth - 14, 12, { align: 'right' });

    doc.setTextColor(...this.theme.text);
    doc.setDrawColor(...this.theme.border);
    doc.setFillColor(...this.theme.surface);
    doc.roundedRect(14, 36, pageWidth - 28, 28, 2, 2, 'FD');

    doc.setFontSize(8);
    doc.setTextColor(...this.theme.muted);
    const metaLines = [
      [`Branch`, context.branchName],
      [`Counter`, context.counterLabel],
      [`Staff`, context.staffName ?? '—'],
      [`Period`, context.periodLabel],
      [`Filters`, context.filtersLabel]
    ];

    let metaY = 42;
    metaLines.forEach(([label, value]) => {
      doc.setFont('helvetica', 'bold');
      doc.setTextColor(...this.theme.muted);
      doc.text(`${label}:`, 18, metaY);
      doc.setFont('helvetica', 'normal');
      doc.setTextColor(...this.theme.text);
      doc.text(this.truncate(String(value), 70), 38, metaY);
      metaY += 4.5;
    });

    return 70;
  }

  private drawSummaryStrip(
    doc: jsPDF,
    startY: number,
    cards: { label: string; value: string; trend?: string }[]
  ): number {
    const pageWidth = doc.internal.pageSize.getWidth();
    const gap = 4;
    const cardWidth = (pageWidth - 28 - gap * (cards.length - 1)) / cards.length;
    let x = 14;

    cards.forEach((card) => {
      doc.setDrawColor(...this.theme.border);
      doc.setFillColor(...this.theme.white);
      doc.roundedRect(x, startY, cardWidth, 22, 2, 2, 'FD');

      doc.setFont('helvetica', 'normal');
      doc.setFontSize(7.5);
      doc.setTextColor(...this.theme.muted);
      doc.text(card.label.toUpperCase(), x + 4, startY + 6);

      doc.setFont('helvetica', 'bold');
      doc.setFontSize(12);
      doc.setTextColor(...this.theme.text);
      doc.text(card.value, x + 4, startY + 13);

      if (card.trend) {
        doc.setFont('helvetica', 'normal');
        doc.setFontSize(7);
        doc.setTextColor(...this.theme.secondary);
        doc.text(card.trend, x + 4, startY + 18);
      }

      x += cardWidth + gap;
    });

    return startY + 28;
  }

  private drawSectionTitle(doc: jsPDF, y: number, title: string): number {
    if (y > 260) {
      doc.addPage();
      y = 20;
    }
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(11);
    doc.setTextColor(...this.theme.text);
    doc.text(title, 14, y);
    doc.setDrawColor(...this.theme.secondary);
    doc.setLineWidth(0.6);
    doc.line(14, y + 2, 60, y + 2);
    return y + 6;
  }

  private drawTipBox(doc: jsPDF, y: number, tip: string): number {
    if (y > 250) {
      doc.addPage();
      y = 20;
    }
    const pageWidth = doc.internal.pageSize.getWidth();
    doc.setDrawColor(...this.theme.primary);
    doc.setFillColor(...this.theme.surface);
    doc.roundedRect(14, y, pageWidth - 28, 24, 2, 2, 'FD');
    doc.setLineWidth(1.2);
    doc.line(14, y, 14, y + 24);

    doc.setFont('helvetica', 'bold');
    doc.setFontSize(9);
    doc.setTextColor(...this.theme.text);
    doc.text('Performance Optimization Tip', 18, y + 7);

    doc.setFont('helvetica', 'normal');
    doc.setFontSize(8);
    doc.setTextColor(...this.theme.muted);
    const lines = doc.splitTextToSize(tip, pageWidth - 40);
    doc.text(lines, 18, y + 13);

    return y + 28;
  }

  private drawPageFooter(doc: jsPDF, pageNumber: number): void {
    const pageWidth = doc.internal.pageSize.getWidth();
    const pageHeight = doc.internal.pageSize.getHeight();
    doc.setFont('helvetica', 'normal');
    doc.setFontSize(7);
    doc.setTextColor(...this.theme.muted);
    doc.text('Smart-Q Management System — Confidential', 14, pageHeight - 8);
    doc.text(`Page ${pageNumber}`, pageWidth - 14, pageHeight - 8, { align: 'right' });
  }

  private triggerDownload(blob: Blob, filename: string): void {
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = filename;
    anchor.click();
    URL.revokeObjectURL(url);
  }

  private fileName(prefix: string, period: string): string {
    const safe = period.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-|-$/g, '');
    const stamp = new Date().toISOString().slice(0, 10);
    return `${prefix}-${safe || 'report'}-${stamp}.pdf`;
  }

  private formatTime(value?: string): string {
    if (!value) return '—';
    const date = new Date(value);
    return Number.isNaN(date.getTime()) ? '—' : date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }

  private formatDateTime(value: string): string {
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return '—';
    return date.toLocaleString([], {
      year: 'numeric',
      month: 'short',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  private truncate(value: string, max: number): string {
    return value.length <= max ? value : `${value.slice(0, max - 1)}…`;
  }
}
