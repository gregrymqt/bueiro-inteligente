import React from 'react';
import { Wallet } from '@mercadopago/sdk-react';
import styles from './PreferencePayment.module.scss';
import { usePreferencePayment } from '../hooks/usePreferencePayment';
import type { PreferenceOnSubmit } from '@mercadopago/sdk-react/esm/bricks/wallet/types';
// Importação do tipo exato disponibilizado pelo SDK para o fluxo onSubmit

interface PreferencePaymentProps {
    planId: string;
    payerEmail?: string;
}

export const PreferencePayment: React.FC<PreferencePaymentProps> = ({ planId, payerEmail }) => {
    const { handleWalletSubmit } = usePreferencePayment(planId);

    // Tipagem exata retirada de PreferenceOnSubmit
    const initialization: PreferenceOnSubmit['initialization'] = {
        redirectMode: 'self', // Redireciona na mesma aba
    };

    // Correção da estrutura: no Wallet Brick, theme, valueProp e customStyle ficam na raiz![cite: 42]
    const customization: PreferenceOnSubmit['customization'] = {
        theme: 'default',
        valueProp: 'security_safety',
        customStyle: {
            buttonBackground: 'default',
            borderRadius: '12px',
            buttonHeight: '52px',
            valuePropColor: 'blue',
        }
    };

    /**
     * O SDK exige que a função retorne uma Promise<unknown> que resolve a string[cite: 42]
     */
    const onSubmit = async (): Promise<unknown> => {
        // Usamos await para que o erro suba para o SDK caso a API falhe
        const preferenceId = await handleWalletSubmit(payerEmail || '');
        return preferenceId;
    };

    const onError = (error: unknown): void => {
        console.error('Wallet Brick Error:', error);
    };

    return (
        <div className={styles.preferenceContainer}>
            <header className={styles.header}>
                <h3>Mercado Pago</h3>
                <p>Use seu saldo, cartão salvo ou Mercado Crédito.</p>
            </header>

            <div className={styles.benefitsBox}>
                <ul>
                    <li>🔒 Compra garantida pelo Mercado Pago</li>
                    <li>⚡ Aprovação imediata com saldo</li>
                    <li>📱 Experiência fluida e segura</li>
                </ul>
            </div>

            <div className={styles.brickWrapper}>
                <Wallet
                    initialization={initialization}
                    customization={customization}
                    onSubmit={onSubmit}
                    onError={onError}
                />
            </div>
        </div>
    );
};