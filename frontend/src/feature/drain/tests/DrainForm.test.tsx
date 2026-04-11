import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { DrainForm } from '../components/DrainForm';

afterEach(() => {
  cleanup();
});

describe('DrainForm', () => {
  it('exibe validações dos campos obrigatórios ao enviar vazio', async () => {
    render(<DrainForm onSubmit={vi.fn()} />);

    fireEvent.click(screen.getByRole('button', { name: /criar bueiro/i }));

    expect(await screen.findByText(/nome do bueiro é obrigatório/i)).toBeInTheDocument();
    expect(screen.getByText(/endereço é obrigatório/i)).toBeInTheDocument();
    expect(screen.getByText(/latitude é obrigatória/i)).toBeInTheDocument();
    expect(screen.getByText(/longitude é obrigatória/i)).toBeInTheDocument();
    expect(screen.getByText(/id do hardware é obrigatório/i)).toBeInTheDocument();
  });

  it('envia o hardware_id e converte o status corretamente', async () => {
    const onSubmit = vi.fn().mockResolvedValue(undefined);

    render(<DrainForm onSubmit={onSubmit} />);

    fireEvent.change(screen.getByLabelText(/nome do bueiro/i), {
      target: { value: 'Bueiro Central' },
    });
    fireEvent.change(screen.getByLabelText(/id do hardware/i), {
      target: { value: 'HW-123' },
    });
    fireEvent.change(screen.getByLabelText(/endereço/i), {
      target: { value: 'Rua Central, 10' },
    });
    fireEvent.change(screen.getByLabelText(/latitude/i), {
      target: { value: '-23.55052' },
    });
    fireEvent.change(screen.getByLabelText(/longitude/i), {
      target: { value: '-46.633308' },
    });
    fireEvent.change(screen.getByLabelText(/status do bueiro/i), {
      target: { value: 'true' },
    });

    fireEvent.click(screen.getByRole('button', { name: /criar bueiro/i }));

    await waitFor(() => expect(onSubmit).toHaveBeenCalledTimes(1));
    expect(onSubmit).toHaveBeenCalledWith(
      expect.objectContaining({
        name: 'Bueiro Central',
        address: 'Rua Central, 10',
        latitude: -23.55052,
        longitude: -46.633308,
        hardware_id: 'HW-123',
        is_active: true,
      })
    );
  });
});